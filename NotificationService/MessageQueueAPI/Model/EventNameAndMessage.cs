// <copyright file="EventNameAndMessage.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

/// <summary>
/// Represents the content of a message to be inserted into a queue.
/// </summary>
public class EventNameAndMessage
{
    /// <summary>
    /// Gets or sets the name of the event.
    /// </summary>
    required public string EventName { get; set; }

    /// <summary>
    /// Gets or sets the message content as a JSON string.
    /// </summary>
    required public string Content { get; set; }
}
