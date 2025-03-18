// <copyright file="IdAndJObjects.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI.Model;

using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a message in the message queue.
/// </summary>
public class IdAndJObject
{
    /// <summary>
    /// Gets or sets the ids of the messages.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the messages as a JSON objects.
    /// </summary>
    required public JObject JObject { get; set; }
}
