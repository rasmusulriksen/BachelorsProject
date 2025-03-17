// <copyright file="QueueTableNames.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

/// <summary>
/// Constants for queue table names used in the notification system.
/// </summary>
public static class QueueTableNames
{
    /// <summary>
    /// Queue table for unprocessed notifications.
    /// </summary>
    public const string UnprocessedNotifications = "queues.unprocessed_notifications";

    /// <summary>
    /// Queue table for emails waiting to be populated with content.
    /// </summary>
    public const string EmailsToBePopulated = "queues.emails_to_be_merged_into_template";

    /// <summary>
    /// Queue table for emails that are ready to be sent.
    /// </summary>
    public const string EmailsToBeSent = "queues.emails_to_be_sent";
}
