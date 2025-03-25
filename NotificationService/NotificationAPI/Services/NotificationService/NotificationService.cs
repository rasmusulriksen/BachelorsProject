// <copyright file="NotificationService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Services.NotificationService;

using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.NotificationAPI.Model;
using Visma.Ims.NotificationAPI.Repositories;

/// <summary>
/// Service for managing notifications.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository repository;
    private readonly ILogFactory logger;
    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="repository">The notification repository.</param>
    /// <param name="logger">The logger factory.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public NotificationService(INotificationRepository repository, ILogFactory logger, IHttpClientFactory httpClientFactory)
    {
        this.repository = repository;
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Gets a notification by its ID.
    /// </summary>
    /// <param name="id">The ID of the notification to get.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The notification.</returns>
    public async Task<Notification> GetByIdAsync(Guid id, string tenantIdentifier)
    {
        this.logger.Log().Information("Getting notification with ID: {Id}", id);
        return await this.repository.GetByIdAsync(id, tenantIdentifier);
    }

    /// <summary>
    /// Gets notifications for a specific user.
    /// </summary>
    /// <param name="userName">The username of the user to get notifications for.</param>
    /// <param name="page">The page number to retrieve.</param>
    /// <param name="pageSize">The number of notifications per page.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The notifications for the user.</returns>
    public async Task<MyNotificationsResponse> GetForUserAsync(string userName, int page, int pageSize, string tenantIdentifier)
    {
        this.logger.Log().Information("Getting notifications for user: {UserName}, page: {Page}, pageSize: {PageSize}", userName, page, pageSize);
        return await this.repository.GetForUserAsync(userName, page, pageSize, tenantIdentifier);
    }

    /// <summary>
    /// Creates a notification.
    /// </summary>
    /// <param name="notificationDTO">The notification to create.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CreateAsync(NotificationFromJavaDto notificationDTO, string tenantIdentifier, CancellationToken cancellationToken)
    {
        this.logger.Log().Information("Creating notification for user: {FeedUserId}", notificationDTO.FeedUserId);

        // 1. Convert to Notification model
        var notification = notificationDTO.ToNotification();

        // 2. Store in database
        await this.repository.CreateAsync(notification, tenantIdentifier);

        // 3. Publish NotificationInitialized event to message queue
        await this.PublishToMessageQueue(notificationDTO, tenantIdentifier, cancellationToken);

        // Set creation timestamp if not provided
        if (notification.PostDate == 0)
        {
            notification.PostDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    /// <summary>
    /// Updates a notification.
    /// </summary>
    /// <param name="notification">The notification to update.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The updated notification.</returns>
    public async Task<Notification> UpdateAsync(Notification notification, string tenantIdentifier)
    {
        this.logger.Log().Information("Updating notification with ID: {Id}", notification.Id);
        return await this.repository.UpdateAsync(notification, tenantIdentifier);
    }

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="id">The ID of the notification to delete.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(Guid id, string tenantIdentifier)
    {
        this.logger.Log().Information("Deleting notification with ID: {Id}", id);
        await this.repository.DeleteAsync(id, tenantIdentifier);
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="id">The ID of the notification to mark as read.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task MarkAsReadAsync(Guid id, string tenantIdentifier)
    {
        this.logger.Log().Information("Marking notification with ID: {Id} as read", id);
        await this.repository.MarkAsReadAsync(id, tenantIdentifier);
    }

    /// <summary>
    /// Gets the unread count for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the unread count for.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The unread count for the user.</returns>
    public async Task<long> GetUnreadCountForUserAsync(string userId, string tenantIdentifier)
    {
        this.logger.Log().Information("Getting unread notification count for user: {UserId}", userId);
        return await this.repository.GetUnreadCountForUserAsync(userId, tenantIdentifier);
    }

    /// <summary>
    /// Gets the total count for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the total count for.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The total count for the user.</returns>
    public async Task<long> GetTotalCountForUserAsync(string userId, string tenantIdentifier)
    {
        this.logger.Log().Information("Getting total notification count for user: {UserId}", userId);
        return await this.repository.GetTotalCountForUserAsync(userId, tenantIdentifier);
    }

    /// <summary>
    /// Creates an email notification.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="linksEnabled">A value indicating whether links are enabled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response body.</returns>
    public async Task<string> CreateEmailNotification(Message message, bool linksEnabled, CancellationToken cancellationToken = default)
    {
        try
        {
            var emailNotification = new EmailNotification(
                message.ActivityType,
                message.JsonData,
                message.UserName,
                message.ToEmail,
                message.FromEmail,
                linksEnabled);

            var client = this.httpClientFactory.CreateClient("MessageQueueClient");

            string url = "http://localhost:5204/messagequeue/publish/EmailTemplateShouldBePopulated";

            var requestBody = new StringContent(
                JsonConvert.SerializeObject(emailNotification),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = requestBody
            };

            var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            return responseBody;
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error sending email notification");
            throw;
        }
    }

    private async Task PublishToMessageQueue(NotificationFromJavaDto notificationDto, string tenantIdentifier, CancellationToken cancellationToken)
    {
        try
        {
            var client = this.httpClientFactory.CreateClient("MessageQueueClient");

            var url = "http://localhost:5204/messagequeue/publish/NotificationInitialized";

            // Instead of using ToMessage(), create a JObject with the exact structure expected by IdAndJObject
            var messageObject = new JObject
            {
                ["activityType"] = notificationDto.ActivityType,
                ["jsonData"] = JObject.FromObject(new JsonData
                {
                    ModifierDisplayName = notificationDto.ActivitySummary["modifierDisplayName"]?.ToString() ?? "Unknown",
                    CaseId = notificationDto.ActivitySummary["caseId"]?.ToString() ?? "Unknown",
                    CaseTitle = notificationDto.ActivitySummary["caseTitle"]?.ToString() ?? "Unknown",
                    DocTitle = notificationDto.ActivitySummary["docTitle"]?.ToString() ?? "Unknown"
                }),
                ["userName"] = notificationDto.FeedUserId,
                ["toEmail"] = notificationDto.ToEmail,
                ["fromEmail"] = notificationDto.FromEmail
            };

            // Serialize using Newtonsoft
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver(),
                Formatting = Formatting.None
            };

            string json = JsonConvert.SerializeObject(messageObject, settings);
            this.logger.Log().Debug("Publishing message to queue: {Json}", json);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            request.Headers.Add("X-Tenant-Identifier", tenantIdentifier);

            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                this.logger.Log().Error(
                    "Failed to publish to message queue. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode,
                    errorContent);
            }
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error publishing to message queue");
            throw;
        }
    }
}
