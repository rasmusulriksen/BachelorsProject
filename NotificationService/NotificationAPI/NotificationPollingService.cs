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
using Visma.Ims.NotificationAPI.Configuration;

/// <summary>
/// Notification polling service.
/// </summary>
public class NotificationPollingService : IRecurringBackgroundService
{
    private readonly ILogFactory logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly NotificationPreferencesConfig preferencesConfig;
    private readonly INotificationService notificationService;

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
        NotificationPreferencesConfig preferencesConfig,
        INotificationService notificationService)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.preferencesConfig = preferencesConfig;
        this.notificationService = notificationService;
    }

    /// <inheritdoc/>
    public string ServiceName => "My Recurring Background Service";

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

                // Add this line to set the Referer header explicitly
                client.DefaultRequestHeaders.Referrer = new Uri("http://localhost:5258");

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
                    var preference = this.preferencesConfig.Preferences.FirstOrDefault(p => p.Email == notification.Message.ToEmail);
                    if (preference != null)
                    {
                        this.logger.Log().Information("Fetched preference for user: {UserName}", preference.Email);

                        bool linksEnabled = preference.LinksEnabled;

                        // Send email notification if enabled
                        if (preference.EmailEnabled)
                        {
                            await this.notificationService.CreateEmailNotification(notification.Message, linksEnabled, cancellationToken);
                        }

                        // Send in-app notification if enabled
                        if (preference.InAppEnabled)
                        {
                            await this.notificationService.CreateInAppNotification(notification.Message, cancellationToken);
                        }

                        // Mark the notification as done
                        // But when is a notification actually done? Who is responsible for updating the status?
                        // Should the processing_status have more states? I.e. "EmailSent", "InAppSent" etc?
                        // Or is the "processing_status" column in the queues.notifications table only related to the processing of the nofitication taking place in this API?
                        // In this case, it's fair to say that the notification is done when it has been processed by this API.
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
