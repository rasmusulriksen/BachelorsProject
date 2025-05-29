// <copyright file="NotificationBackgroundService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI;

using Model;
using Newtonsoft.Json;
using Visma.Ims.Common.Infrastructure.HostedService;
using Visma.Ims.Common.Infrastructure.Logging;
using Visma.Ims.NotificationAPI.Services.NotificationPreferencesService;
using Visma.Ims.NotificationAPI.Services.NotificationService;
using System.Net.Http.Headers;
using System.Net.Http;

/// <summary>
/// Notification polling service.
/// </summary>
public class NotificationBackgroundService : IRecurringBackgroundService
{
    private readonly ILogFactory logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly INotificationService notificationService;
    private readonly INotificationPreferencesService notificationPreferencesService;
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationBackgroundService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="notificationPreferencesService">Service for managing notification preferences.</param>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="configuration">The configuration.</param>
    public NotificationBackgroundService(
        ILogFactory logger,
        IHttpClientFactory httpClientFactory,
        INotificationPreferencesService notificationPreferencesService,
        INotificationService notificationService,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.notificationPreferencesService = notificationPreferencesService;
        this.notificationService = notificationService;
        this.configuration = configuration;
    }

    /// <inheritdoc/>
    public string ServiceName => "NotificationBackgroundService";

    /// <inheritdoc/>
    public virtual TimeSpan RunEvery => TimeSpan.FromSeconds(30);

    /// <inheritdoc/>
    public virtual TimeSpan CancelAfter => TimeSpan.FromSeconds(60);

    /// <inheritdoc/>
    public async Task DoWork(CancellationToken cancellationToken)
    {
        this.logger.Log().Information("NotificationBackgroundService started.");

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

                // Process each tenant in parallel
                var tasks = tenants.Select(async tenantInfo =>
                {
                    this.logger.Log().Information($"Polling for: {tenantInfo.TenantUrl}");

                    var client = this.httpClientFactory.CreateClient("MessageQueueClient");
                    client.DefaultRequestHeaders.Add("X-Tenant-Identifier", tenantInfo.TenantIdentifier);

                    // Create request message with Referer header
                    var request = new HttpRequestMessage(HttpMethod.Get, "messagequeue/poll");
                    request.Headers.Referrer = new Uri("http://notification-api:8080");

                    var response = await client.SendAsync(request, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        this.logger.Log().Warning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                        return;
                    }

                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var responseJObjects = JsonConvert.DeserializeObject<List<IdAndJObject>>(responseContent);
                    var notifications = responseJObjects.Select(n => n.ToIdAndMessage()).ToList();

                    foreach (var notification in notifications)
                    {
                        // Find user preference
                        NotificationPreference preference = await this.notificationPreferencesService.GetNotificationPreferenceObjectByUsernameAsync(notification.Message.UserName, tenantInfo.TenantIdentifier);

                        if (preference == null)
                        {
                            this.logger.Log().Warning("No preference found for user: {UserName}", notification.Message.UserName);
                            continue;
                        }

                        if (preference != null)
                        {
                            this.logger.Log().Information("Fetched preference for user: {UserName}", preference.UserName);

                            bool linksEnabled = preference.LinksEnabled;

                            // Send email notification if enabled
                            if (preference.EmailEnabled)
                            {
                                await this.notificationService.CreateEmailNotification(notification.Message, linksEnabled, tenantInfo.TenantIdentifier, cancellationToken);
                            }

                            // Mark the notification as done
                            var doneRequest = new HttpRequestMessage(HttpMethod.Get, $"messagequeue/done/{notification.Id}");
                            doneRequest.Headers.Referrer = new Uri("http://notification-api:8080");
                            await client.SendAsync(doneRequest, cancellationToken);
                        }
                    }
                });
                // Wait for all clients to be processed
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                this.logger.Log().Error(ex, "Error occurred while polling for or processing notifications.");
            }

            await Task.Delay(this.RunEvery, cancellationToken);
        }

        this.logger.Log().Information("Email polling service is stopping.");
    }
}
