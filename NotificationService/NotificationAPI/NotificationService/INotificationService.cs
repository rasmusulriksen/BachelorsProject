// <copyright file="INotificationService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI;

using Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Notification service interface.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates an email notification.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="linksEnabled">A value indicating whether links are enabled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response body.</returns>
    Task<string> CreateEmailNotification(Message message, bool linksEnabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an in-app notification.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response body.</returns>
    Task CreateInAppNotification(Message message, CancellationToken cancellationToken = default);
}
