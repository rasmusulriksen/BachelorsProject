// <copyright file="EmailSenderBackgroundService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailSenderAPI;

using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Visma.Ims.Common.Abstractions.HostedService;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.EmailSenderAPI.Model;
using Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Represents the email sender background service.
/// </summary>
public class EmailSenderBackgroundService : IRecurringBackgroundService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogFactory logger;
    private readonly IEmailSenderService emailSenderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSenderBackgroundService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="emailSenderService">The email sender service.</param>
    public EmailSenderBackgroundService(
        IHttpClientFactory httpClientFactory,
        ILogFactory logger,
        IEmailSenderService emailSenderService)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        this.emailSenderService = emailSenderService;
    }

    /// <inheritdoc/>
    public string ServiceName => "EmailSenderBackgroundService";

    /// <inheritdoc/>
    public virtual TimeSpan RunEvery => TimeSpan.FromSeconds(10);

    /// <inheritdoc/>
    public virtual TimeSpan CancelAfter => TimeSpan.FromSeconds(56);

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DoWork(CancellationToken cancellationToken)
    {
        this.logger.Log().Information("EmailSenderBackgroundService started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = this.httpClientFactory.CreateClient("MessageQueueClient");

                var response = await client.GetAsync("http://localhost:5170/messagequeue/poll", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    this.logger.Log().Warning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var idAndJObjects = JsonConvert.DeserializeObject<List<IdAndJObject>>(responseContent);
                var outboundEmails = idAndJObjects.Select(idAndJObject => idAndJObject.ToIdAndOutboundEmail()).ToList();

                foreach (var outboundEmail in outboundEmails)
                {
                    try
                    {
                        this.logger.Log().Information("Processing email: Subject={Subject}, To={ToEmail}", outboundEmail.OutboundEmail.Subject, outboundEmail.OutboundEmail.ToEmail);

                        await this.emailSenderService.SendEmailAsync(outboundEmail.OutboundEmail.Subject, outboundEmail.OutboundEmail.HtmlBody);

                        this.logger.Log().Information("Email sent successfully");

                        // Mark as done
                        await client.GetAsync($"http://localhost:5204/messagequeue/done/{outboundEmail.Id}", cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log().Error(ex, "Error processing email message {MessageId}", outboundEmail.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Log().Error(ex, "Error occurred while polling for or processing emails.");
            }

            await Task.Delay(this.RunEvery, cancellationToken);
        }

        this.logger.Log().Information("Email sender polling service is stopping.");
    }
}
