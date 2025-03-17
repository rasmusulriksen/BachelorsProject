// <copyright file="EventNameToDbTableMapper.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

/// <summary>
/// This class maps event names to the corresponding database table names.
/// </summary>
public static class EventNameToDbTableMapper
{
    /// <summary>
    /// Gets the database table name for the given event name.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>The string representation of the database table name.</returns>
    public static string GetDbTableForEventName(string eventName)
    {
        return eventName switch
        {
            EventNames.NotificationInitialized => QueueTableNames.UnprocessedNotifications,
            EventNames.EmailTemplateShouldBePopulated => QueueTableNames.EmailsToBePopulated,
            EventNames.EmailTemplateHasBeenPopulated => QueueTableNames.EmailsToBeSent,
            _ => throw new ArgumentException($"Unknown event name: {eventName}")
        };
    }
}