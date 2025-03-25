// <copyright file="NotificationPollingService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI;

using Model;
using Newtonsoft.Json;
using Visma.Ims.Common.Abstractions.HostedService;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.NotificationAPI.Services.NotificationPreferencesService;
using Visma.Ims.NotificationAPI.Services.NotificationService;

/// <summary>
/// Notification polling service.
/// </summary>
public class NotificationPollingService : IRecurringBackgroundService
{
    private readonly ILogFactory logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly INotificationService notificationService;
    private readonly INotificationPreferencesService notificationPreferencesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPollingService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="preferencesConfig">The preferences config.</param>
    /// <param name="notificationService">The notification service.</param>
    public NotificationPollingService(
        ILogFactory logger,
        IHttpClientFactory httpClientFactory,
        INotificationPreferencesService notificationPreferencesService,
        INotificationService notificationService)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.notificationPreferencesService = notificationPreferencesService;
        this.notificationService = notificationService;
    }

    /// <inheritdoc/>
    public string ServiceName => "NotificationPollingService";

    /// <inheritdoc/>
    public virtual TimeSpan RunEvery => TimeSpan.FromSeconds(10);

    /// <inheritdoc/>
    public virtual TimeSpan CancelAfter => TimeSpan.FromSeconds(56);

    /// <inheritdoc/>
    public async Task DoWork(CancellationToken cancellationToken)
    {
        this.logger.Log().Information("NotificationPollingService started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = this.httpClientFactory.CreateClient("MessageQueueClient");

                var response = await client.GetAsync("http://localhost:5204/api/messagequeue/poll", cancellationToken);

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
                    NotificationPreference preference = await this.notificationPreferencesService.GetNotificationPreferenceObjectByUsernameAsync(notification.Message.UserName);

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
                            await this.notificationService.CreateEmailNotification(notification.Message, linksEnabled, cancellationToken);
                        }

                        // Mark the notification as done
                        await client.GetAsync($"http://localhost:5204/api/messagequeue/done/{notification.Id}", cancellationToken);
                    }
                }
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
