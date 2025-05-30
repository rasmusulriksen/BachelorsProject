// <copyright file="NotificationController.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Visma.Ims.Common.Infrastructure.Logging;
using Visma.Ims.NotificationAPI.Model;
using Visma.Ims.NotificationAPI.Services.NotificationService;

/// <summary>
/// Controller for managing notifications.
/// </summary>
[ApiController]
[Route("[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService service;
    private readonly ILogFactory logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationController"/> class.
    /// </summary>
    /// <param name="service">The notification service.</param>
    /// <param name="logger">The logger factory.</param>
    public NotificationController(INotificationService service, ILogFactory logger)
    {
        this.service = service;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a notification by its ID.
    /// </summary>
    /// <param name="id">The ID of the notification to retrieve.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The notification if found, otherwise a NotFound result.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetById(Guid id, [FromHeader(Name = "X-Tenant-Identifier")] string tenantIdentifier)
    {
        if (string.IsNullOrEmpty(tenantIdentifier))
        {
            return this.BadRequest("X-Tenant-Identifier header is required");
        }

        var notification = await this.service.GetByIdAsync(id, tenantIdentifier);
        if (notification == null)
        {
            return this.NotFound();
        }

        return this.Ok(notification);
    }

    /// <summary>
    /// Gets notifications for a specific user.
    /// </summary>
    /// <param name="userName">The username of the user to get notifications for.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The notifications for the user.</returns>
    [HttpGet("user/{userName}")]
    public async Task<ActionResult<MyNotificationsResponse>> GetForUser(string userName, [FromHeader(Name = "X-Tenant-Identifier")] string tenantIdentifier)
    {
        if (string.IsNullOrEmpty(tenantIdentifier))
        {
            return this.BadRequest("X-Tenant-Identifier header is required");
        }

        var page = 1;
        var pageSize = 100;
        var response = await this.service.GetForUserAsync(userName, page, pageSize, tenantIdentifier);
        return this.Ok(response);
    }

    /// <summary>
    /// Updates a notification.
    /// </summary>
    /// <param name="id">The ID of the notification to update.</param>
    /// <param name="notification">The updated notification.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>Updated notification.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<Notification>> Update(Guid id, Notification notification, [FromHeader(Name = "X-Tenant-Identifier")] string tenantIdentifier)
    {
        if (string.IsNullOrEmpty(tenantIdentifier))
        {
            return this.BadRequest("X-Tenant-Identifier header is required");
        }

        if (notification == null || id != notification.Id)
        {
            return this.BadRequest();
        }

        var existingNotification = await this.service.GetByIdAsync(id, tenantIdentifier);
        if (existingNotification == null)
        {
            return this.NotFound();
        }

        var updated = await this.service.UpdateAsync(notification, tenantIdentifier);
        return this.Ok(updated);
    }

    /// <summary>
    /// Deletes a notification by its ID.
    /// </summary>
    /// <param name="id">The ID of the notification to delete.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>No content if the notification is deleted, otherwise a NotFound result.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, [FromHeader(Name = "X-Tenant-Identifier")] string tenantIdentifier)
    {
        if (string.IsNullOrEmpty(tenantIdentifier))
        {
            return this.BadRequest("X-Tenant-Identifier header is required");
        }

        var notification = await this.service.GetByIdAsync(id, tenantIdentifier);
        if (notification == null)
        {
            return this.NotFound();
        }

        await this.service.DeleteAsync(id, tenantIdentifier);
        return this.NoContent();
    }

    /// <summary>
    /// Marks a notification as read by its ID.
    /// </summary>
    /// <param name="id">The ID of the notification to mark as read.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>No content if the notification is marked as read, otherwise a NotFound result.</returns>
    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(Guid id, [FromHeader(Name = "X-Tenant-Identifier")] string tenantIdentifier)
    {
        if (string.IsNullOrEmpty(tenantIdentifier))
        {
            return this.BadRequest("X-Tenant-Identifier header is required");
        }

        var notification = await this.service.GetByIdAsync(id, tenantIdentifier);
        if (notification == null)
        {
            return this.NotFound();
        }

        await this.service.MarkAsReadAsync(id, tenantIdentifier);
        return this.NoContent();
    }

    /// <summary>
    /// Gets the unread count for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the unread count for.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The unread count for the user.</returns>
    [HttpGet("user/{userId}/unread-count")]
    public async Task<ActionResult<object>> GetUnreadCountForUser(string userId, [FromHeader(Name = "X-Tenant-Identifier")] string tenantIdentifier)
    {
        if (string.IsNullOrEmpty(tenantIdentifier))
        {
            return this.BadRequest("X-Tenant-Identifier header is required");
        }

        var count = await this.service.GetUnreadCountForUserAsync(userId, tenantIdentifier);
        return this.Ok(count);
    }

    /// <summary>
    /// Gets the total count for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the total count for.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The total count for the user.</returns>
    [HttpGet("user/{userId}/total-count")]
    public async Task<ActionResult<object>> GetTotalCountForUser(string userId, [FromHeader(Name = "X-Tenant-Identifier")] string tenantIdentifier)
    {
        if (string.IsNullOrEmpty(tenantIdentifier))
        {
            return this.BadRequest("X-Tenant-Identifier header is required");
        }

        var count = await this.service.GetTotalCountForUserAsync(userId, tenantIdentifier);
        return this.Ok(new { totalCount = count });
    }

    /// <summary>
    /// Creates a notification. This is initialized from the Java monolith.
    /// </summary>
    /// <param name="notificationDto">The notification data from Java.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created notification.</returns>
    [HttpPost]
    public async Task<ActionResult> CreateNotification(
        [FromBody] NotificationFromJavaDto notificationDto,
        [FromHeader(Name = "X-Tenant-Identifier")] string tenantIdentifier,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(tenantIdentifier))
        {
            return this.BadRequest("X-Tenant-Identifier header is required");
        }

        try
        {
            if (notificationDto == null)
            {
                return this.BadRequest("Request body cannot be null");
            }

            this.logger.Log().Information(
                "Received notification from Java for user {FeedUserId}, activity {ActivityType}",
                notificationDto.FeedUserId,
                notificationDto.ActivityType);

            await this.service.CreateAsync(notificationDto, tenantIdentifier, cancellationToken);

            return this.NoContent();
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(
                ex,
                "Error processing notification from Java for user {FeedUserId}",
                notificationDto?.FeedUserId);
            return this.StatusCode(500, "An error occurred while processing the notification");
        }
    }
}
