// <copyright file="EmailTemplateBackgroundService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailTemplateAPI;

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Visma.Ims.Common.Infrastructure.HostedService;
using Visma.Ims.Common.Infrastructure.Logging;
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
    private readonly IConfiguration configuration;
    private readonly IEmailTemplateService emailTemplateService;
    private readonly IWebHostEnvironment hostEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailTemplateBackgroundService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="emailTemplateService">The email template service.</param>
    /// <param name="hostEnvironment">The web host environment.</param>
    public EmailTemplateBackgroundService(
        ILogFactory logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IEmailTemplateService emailTemplateService,
        IWebHostEnvironment hostEnvironment)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.configuration = configuration;
        this.emailTemplateService = emailTemplateService;
        this.hostEnvironment = hostEnvironment;

        // Register Handlebars helpers and partials
        HandlebarsHelperExtensions.RegisterAllHelpersAndPartials(this.hostEnvironment, logger);
    }

    /// <inheritdoc/>
    public string ServiceName => "EmailTemplateBackgroundService";

    /// <inheritdoc/>
    public virtual TimeSpan RunEvery => TimeSpan.FromSeconds(30);

    /// <inheritdoc/>
    public virtual TimeSpan CancelAfter => TimeSpan.FromSeconds(60);

    /// <inheritdoc/>
    public async Task DoWork(CancellationToken cancellationToken)
    {
        this.logger.Log().Information("EmailTemplatePollingService started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Get all tenants
                List<TenantInfo> tenants = this.configuration.GetSection("Tenants").Get<List<TenantInfo>>();

                if (!tenants.Any())
                {
                    this.logger.Log().Warning("No tenants found in configuration");
                    await Task.Delay(this.RunEvery, cancellationToken);
                    continue;
                }

                // Process each tenant sequentially to avoid too many database connections
                foreach (var tenantInfo in tenants)
                {
                    try 
                    {
                        this.logger.Log().Information($"Polling for: {tenantInfo.TenantUrl}");

                        // Poll for messages for this tenant
                        List<IdAndEmailActivity> emailActivities = await this.PollForNewMessagesAsync(tenantInfo.TenantIdentifier, cancellationToken);

                        if (!emailActivities.Any())
                        {
                            this.logger.Log().Information("No messages found for tenant {TenantIdentifier}", tenantInfo.TenantIdentifier);
                            continue;
                        }

                        this.logger.Log().Information("Processing {Count} messages for tenant {TenantIdentifier}", emailActivities.Count, tenantInfo.TenantIdentifier);

                        // Process each message for this tenant
                        foreach (IdAndEmailActivity emailActivity in emailActivities)
                        {
                            try
                            {
                                await this.ProcessEmailActivityAsync(emailActivity, tenantInfo, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                this.logger.Log().Error(ex, "Error processing email activity {EmailActivityId} for tenant {TenantIdentifier}",
                                    emailActivity.Id, tenantInfo.TenantIdentifier);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log().Error(ex, "Error processing tenant {TenantIdentifier}", tenantInfo.TenantIdentifier);
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

    private async Task<List<IdAndEmailActivity>> PollForNewMessagesAsync(string tenantIdentifier, CancellationToken cancellationToken)
    {
        try
        {
            var client = this.httpClientFactory.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "http://message-queue-api:8080/messagequeue/poll");
            request.Headers.Add("X-Tenant-Identifier", tenantIdentifier);
            request.Headers.Referrer = new Uri("http://email-template-api:8080");

            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                this.logger.Log().Warning("No new emails or processing failed: {StatusCode}, Tenant: {TenantIdentifier}", 
                    response.StatusCode, tenantIdentifier);
                return new List<IdAndEmailActivity>();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var idAndJObjects = JsonConvert.DeserializeObject<List<IdAndJObject>>(responseContent);
            var emailActivities = idAndJObjects?.Select(j => j.ToIdAndEmailActivity()).ToList();

            return emailActivities ?? new List<IdAndEmailActivity>();
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error polling for messages for tenant {TenantIdentifier}", tenantIdentifier);
            return new List<IdAndEmailActivity>();
        }
    }

    private async Task ProcessEmailActivityAsync(IdAndEmailActivity emailActivity, TenantInfo tenantInfo, CancellationToken cancellationToken)
    {
        // Default language - could be extracted from message or user preferences
        var userLanguage = "en";

        try
        {
            OutboundEmail outboundEmailMessage = await this.emailTemplateService.ProcessEmailTemplateAsync(emailActivity.EmailActivity, userLanguage, cancellationToken);

            bool publishSuccess = await this.emailTemplateService.PublishProcessedEmailAsync(outboundEmailMessage, emailActivity.Id, tenantInfo, cancellationToken);

            if (publishSuccess)
            {
                await this.MarkMessageAsDoneAsync(emailActivity.Id, tenantInfo, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error processing email activity {EmailActivityId}", emailActivity.Id);
            throw;
        }
    }

    private async Task MarkMessageAsDoneAsync(long messageId, TenantInfo tenantInfo, CancellationToken cancellationToken)
    {
        try
        {
            var client = this.httpClientFactory.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, $"http://message-queue-api:8080/messagequeue/done/{messageId}");
            request.Headers.Add("X-Tenant-Identifier", tenantInfo.TenantIdentifier);
            request.Headers.Referrer = new Uri("http://email-template-api:8080");

            var response = await client.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                this.logger.Log().Warning("Failed to mark message as done: {StatusCode}, MessageId: {MessageId}, Tenant: {TenantIdentifier}",
                    response.StatusCode, messageId, tenantInfo.TenantIdentifier);
            }
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error marking message {MessageId} as done for tenant {TenantIdentifier}", 
                messageId, tenantInfo.TenantIdentifier);
            throw;
        }
    }
}
