// <copyright file="InAppNotification.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Represents an in-app notification.
/// </summary>
public class InAppNotification
{
    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    required public string ActivityType { get; set; }

    /// <summary>
    /// Gets or sets the JSON data.
    /// </summary>
    required public JsonData JsonData { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// In queues.unprocessed_notifications this is called "UserName", but Alfresco's activityService.postActivity uses "userId"
    /// I want to change this to "UserName" for consistency, but I am not yet sure if this will break anything
    /// </summary>
    required public string UserName { get; set; }
}
