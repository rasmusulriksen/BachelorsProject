// <copyright file="NotificationController.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Notification controller.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationService notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationController"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    public NotificationController(INotificationService notificationService)
    {
        this.notificationService = notificationService;
    }
}
