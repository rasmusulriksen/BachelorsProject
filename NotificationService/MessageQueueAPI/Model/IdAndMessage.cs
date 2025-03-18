// <copyright file="IdAndMessage.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

/// <summary>
/// Represents a message in the message queue.
/// </summary>
public class IdAndMessage
{
    /// <summary>
    /// Gets or sets the id of the message.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the message json string (this is where the actual message data is stored).
    /// </summary>
    required public string Message { get; set; }
}
