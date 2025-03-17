// <copyright file="EventNames.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

/// <summary>
/// Constants for event names used in the notification system.
/// </summary>
public static class EventNames
{
    /// <summary>
    /// Event triggered when a notification is initialized.
    /// </summary>
    public const string NotificationInitialized = "NotificationInitialized";

    /// <summary>
    /// Event triggered when an email template should be populated.
    /// </summary>
    public const string EmailTemplateShouldBePopulated = "EmailTemplateShouldBePopulated";

    /// <summary>
    /// Event triggered when an email template has been populated.
    /// </summary>
    public const string EmailTemplateHasBeenPopulated = "EmailTemplateHasBeenPopulated";
}
