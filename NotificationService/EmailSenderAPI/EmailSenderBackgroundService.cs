// <copyright file="EmailSenderBackgroundService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailSenderAPI;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Visma.Ims.EmailSenderAPI.Configuration;
using Visma.Ims.EmailSenderAPI.Model;
using Visma.Ims.Common.Infrastructure.HostedService;
using Visma.Ims.Common.Infrastructure.Logging;
using Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Represents the email sender background service.
/// </summary>
public class EmailSenderBackgroundService : IRecurringBackgroundService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogFactory logger;
    private readonly IEmailSenderService emailSenderService;
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSenderBackgroundService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="emailSenderService">The email sender service.</param>
    /// <param name="configuration">The configuration.</param>
    public EmailSenderBackgroundService(
        IHttpClientFactory httpClientFactory,
        ILogFactory logger,
        IEmailSenderService emailSenderService,
        IConfiguration configuration)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        this.emailSenderService = emailSenderService;
        this.configuration = configuration;
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
                // Get all tenants
                List<TenantInfo> tenants = this.configuration.GetSection("Tenants").Get<List<TenantInfo>>();

                if (!tenants.Any())
                {
                    this.logger.Log().Warning("No tenants found in configuration");
                    await Task.Delay(this.RunEvery, cancellationToken);
                    continue;
                }

                // Process each tenant sequentially
                foreach (var tenantInfo in tenants)
                {
                    try
                    {
                        this.logger.Log().Information($"Polling for: {tenantInfo.TenantUrl}");

                        var client = this.httpClientFactory.CreateClient("MessageQueueClient");

                        var request = new HttpRequestMessage(HttpMethod.Get, "http://message-queue-api:8080/messagequeue/poll");
                        request.Headers.Add("X-Tenant-Identifier", tenantInfo.TenantIdentifier);
                        request.Headers.Referrer = new Uri("http://email-sender-api:8080");
                        var response = await client.SendAsync(request, cancellationToken);

                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                            this.logger.Log().Warning("No new emails or processing failed: {StatusCode}, Tenant: {TenantIdentifier}, Error: {Error}", 
                                response.StatusCode, tenantInfo.TenantIdentifier, errorContent);
                            continue;
                        }

                        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        
                        // Check if the response content is empty or null before deserializing
                        if (string.IsNullOrWhiteSpace(responseContent))
                        {
                            this.logger.Log().Information("No messages found for tenant {TenantIdentifier}", tenantInfo.TenantIdentifier);
                            continue;
                        }

                        var idAndJObjects = JsonConvert.DeserializeObject<List<IdAndJObject>>(responseContent);
                        
                        if (idAndJObjects == null || !idAndJObjects.Any())
                        {
                            this.logger.Log().Information("No messages found for tenant {TenantIdentifier}", tenantInfo.TenantIdentifier);
                            continue;
                        }

                        var outboundEmails = idAndJObjects.Select(idAndJObject => idAndJObject.ToIdAndOutboundEmail()).ToList();

                        this.logger.Log().Information("Processing {Count} emails for tenant {TenantIdentifier}", outboundEmails.Count, tenantInfo.TenantIdentifier);

                        foreach (var outboundEmail in outboundEmails)
                        {
                            try
                            {
                                this.logger.Log().Information("Processing email: Subject={Subject}, To={ToEmail}, Tenant={TenantIdentifier}", 
                                    outboundEmail.OutboundEmail.Subject, outboundEmail.OutboundEmail.ToEmail, tenantInfo.TenantIdentifier);

                                await this.emailSenderService.SendEmailAsync(outboundEmail.OutboundEmail);

                                this.logger.Log().Information("Email sent successfully");

                                // Mark as done
                                var doneRequest = new HttpRequestMessage(HttpMethod.Get, $"http://message-queue-api:8080/messagequeue/done/{outboundEmail.Id}");
                                doneRequest.Headers.Add("X-Tenant-Identifier", tenantInfo.TenantIdentifier);
                                doneRequest.Headers.Referrer = new Uri("http://email-sender-api:8080");
                                await client.SendAsync(doneRequest, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                this.logger.Log().Error(ex, "Error processing email message {MessageId} for tenant {TenantIdentifier}", 
                                    outboundEmail.Id, tenantInfo.TenantIdentifier);
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
                this.logger.Log().Error(ex, "Error occurred while polling for or processing emails.");
            }

            await Task.Delay(this.RunEvery, cancellationToken);
        }

        this.logger.Log().Information("Email sender polling service is stopping.");
    }
}
