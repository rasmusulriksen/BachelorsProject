// <copyright file="INotificationRepository.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Repositories;

using Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Repository for notification data access.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Gets a notification by its ID.
    /// </summary>
    /// <param name="id">The ID of the notification to retrieve.</param>
    /// <returns>The notification if found, otherwise a NotFound result.</returns>
    Task<Notification> GetByIdAsync(Guid id, string tenantIdentifier);

    /// <summary>
    /// Gets notifications for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get notifications for.</param>
    /// <param name="page">The page number to retrieve.</param>
    /// <param name="pageSize">The number of notifications per page.</param>
    /// <returns>The notifications for the user.</returns>
    Task<MyNotificationsResponse> GetForUserAsync(string userId, int page, int pageSize, string tenantIdentifier);

    /// <summary>
    /// Creates a notification.
    /// </summary>
    /// <param name="notification">The notification to create.</param>
    /// <returns>The created notification.</returns>
    Task CreateAsync(Notification notification, string tenantIdentifier);

    /// <summary>
    /// Updates a notification.
    /// </summary>
    /// <param name="notification">The notification to update.</param>
    /// <returns>The updated notification.</returns>
    Task<Notification> UpdateAsync(Notification notification, string tenantIdentifier);

    /// <summary>
    /// Deletes a notification by its ID.
    /// </summary>
    /// <param name="id">The ID of the notification to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id, string tenantIdentifier);

    /// <summary>
    /// Marks a notification as read by its ID.
    /// </summary>
    /// <param name="id">The ID of the notification to mark as read.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkAsReadAsync(Guid id, string tenantIdentifier);

    /// <summary>
    /// Gets the unread count for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the unread count for.</param>
    /// <returns>The unread count for the user.</returns>
    Task<long> GetUnreadCountForUserAsync(string userId, string tenantIdentifier);

    /// <summary>
    /// Gets the total count for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the total count for.</param>
    /// <returns>The total count for the user.</returns>
    Task<long> GetTotalCountForUserAsync(string userId, string tenantIdentifier);
}
