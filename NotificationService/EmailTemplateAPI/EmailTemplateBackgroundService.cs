// <copyright file="EmailTemplateBackgroundService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailTemplateAPI;

using System.Text.Json.Serialization;
using Visma.Ims.Common.Abstractions.HostedService;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.EmailTemplateAPI.Handlebars;
using Visma.Ims.EmailTemplateAPI.Model;
using Visma.Ims.NotificationAPI.Model;
using Newtonsoft.Json;

/// <summary>
/// Background service for processing email templates.
/// </summary>
public class EmailTemplateBackgroundService : IRecurringBackgroundService
{
    private readonly ILogFactory logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IEmailTemplateService emailTemplateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailTemplateBackgroundService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="hostEnvironment">The host environment.</param>
    /// <param name="emailTemplateService">The email template service.</param>
    public EmailTemplateBackgroundService(
        IHttpClientFactory httpClientFactory,
        ILogFactory logger,
        IHostEnvironment hostEnvironment,
        IEmailTemplateService emailTemplateService)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        this.emailTemplateService = emailTemplateService;

        // Register Handlebars helpers and partials
        HandlebarsHelperExtensions.RegisterAllHelpersAndPartials(hostEnvironment, logger);
    }

    /// <inheritdoc/>
    public string ServiceName => "EmailTemplateBackgroundService";

    /// <inheritdoc/>
    public virtual TimeSpan RunEvery => TimeSpan.FromSeconds(10);

    /// <inheritdoc/>
    public virtual TimeSpan CancelAfter => TimeSpan.FromSeconds(56);

    /// <inheritdoc/>
    public async Task DoWork(CancellationToken cancellationToken)
    {
        this.logger.Log().Information("EmailTemplatePollingService started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = this.httpClientFactory.CreateClient("MessageQueueClient");

                List<IdAndEmailActivity> emailActivities = await this.PollForNewMessagesAsync(client, cancellationToken);

                if (!emailActivities.Any())
                {
                    await Task.Delay(this.RunEvery, cancellationToken);
                    continue;
                }

                foreach (IdAndEmailActivity emailActivity in emailActivities)
                {
                    try
                    {
                        await this.ProcessEmailActivityAsync(emailActivity, client, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log().Error(ex, "Error processing email activity {EmailActivityId}", emailActivity.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Log().Error(ex, "Error occurred in email template polling service.");
            }

            await Task.Delay(this.RunEvery, cancellationToken);
        }

        this.logger.Log().Information("Email template polling service is stopping.");
    }

    private async Task<List<IdAndEmailActivity>?> PollForNewMessagesAsync(HttpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetAsync("http://localhost:5204/api/messagequeue/poll", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                this.logger.Log().Warning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var idAndJObjects = JsonConvert.DeserializeObject<List<IdAndJObject>>(responseContent);
            var emailActivities = idAndJObjects?.Select(j => j.ToIdAndEmailActivity()).ToList();

            return emailActivities;
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error polling for new messages");
            return null;
        }
    }

    private async Task ProcessEmailActivityAsync(IdAndEmailActivity emailActivity, HttpClient client, CancellationToken cancellationToken)
    {
        // Default language - could be extracted from message or user preferences
        var userLanguage = "en";

        try
        {

            OutboundEmail outboundEmailMessage = await this.emailTemplateService.ProcessEmailTemplateAsync(emailActivity.EmailActivity, userLanguage, cancellationToken);

            bool publishSuccess = await this.emailTemplateService.PublishProcessedEmailAsync(outboundEmailMessage, emailActivity.Id, client, cancellationToken);

            if (publishSuccess)
            {
                await this.MarkMessageAsDoneAsync(client, emailActivity.Id, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error processing email activity {EmailActivityId}", emailActivity.Id);
            throw;
        }
    }

    private async Task MarkMessageAsDoneAsync(HttpClient client, long messageId, CancellationToken cancellationToken)
    {
        try
        {

            await client.GetAsync(
                $"http://localhost:5204/api/messagequeue/done/{messageId}", cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error marking message {MessageId} as done", messageId);
            throw;
        }
    }
}
