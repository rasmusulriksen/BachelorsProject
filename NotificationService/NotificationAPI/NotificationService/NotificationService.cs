// <copyright file="NotificationService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI;

using System.Text.Json;
using System.Text;
using Visma.Ims.NotificationAPI.Model;
using Visma.Ims.Common.Abstractions.Logging;

/// <summary>
/// Notification service implementation.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogFactory logger;
    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public NotificationService(ILogFactory logger, IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
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
        // transform Message to EmailNotification (the difference is that EmailNotification has an additional LinksEnabled bool prop)
        var emailNotification = new EmailNotification(
            message.ActivityType,
            message.JsonData,
            message.UserName,
            message.ToEmail,
            message.FromEmail,
            linksEnabled);

        try
        {
            var client = this.httpClientFactory.CreateClient("MessageQueueClient");
            string url = "http://localhost:5204/api/messagequeue/publish/EmailTemplateShouldBePopulated";

            var emailContent = new StringContent(JsonSerializer.Serialize(emailNotification), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, emailContent, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            this.logger.Log().Information("Email sent: {ResponseBody}", responseBody);
            return responseBody;
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error sending email notification");
            throw;
        }
    }

    /// <summary>
    /// Creates an in-app notification.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response body.</returns>
    public async Task CreateInAppNotification(Message message, CancellationToken cancellationToken = default)
    {
        try
        {
            var inAppNotification = new InAppNotification
            {
                ActivityType = message.ActivityType,
                JsonData = message.JsonData,
                UserName = message.UserName
            };

            var serializedInAppNotification = JsonSerializer.Serialize(inAppNotification);
            var inAppNotificationContent = new StringContent(serializedInAppNotification, Encoding.UTF8, "application/json");

            var url = "http://client1.imscase.dk:8080/alfresco/wcs/api/openesdh/notifications";
            var client = this.httpClientFactory.CreateClient("InAppNotificationClient");
            await client.PostAsync(url, inAppNotificationContent, cancellationToken);

            this.logger.Log().Information("In-app notification sent");
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error sending in-app notification");
        }
    }
}
