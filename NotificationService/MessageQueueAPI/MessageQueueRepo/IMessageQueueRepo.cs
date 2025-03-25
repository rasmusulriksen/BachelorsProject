// <copyright file="IMessageQueueRepo.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Visma.Ims.NotificationService.MessageQueueAPI.Model;

/// <summary>
/// Interface for the message queue repository.
/// </summary>
public interface IMessageQueueRepo
{
    /// <summary>
    /// Enqueues a message to the message queue.
    /// </summary>
    /// <param name="message">The message to publish as a JSON object.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The id of the inserted message.</returns>
    Task<long> EnqueueMessage(JObject message, string eventName, string tenantIdentifier);

    /// <summary>
    /// Dequeues messages from the message queue.
    /// </summary>
    /// <param name="referer">The referer.</param>
    /// <param name="count">The number of messages to dequeue.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The list of dequeued messages.</returns>
    Task<IEnumerable<IdAndJObject>> DequeueMessages(string referer, int count, string tenantIdentifier);

    /// <summary>
    /// Marks a message as done.
    /// </summary>
    /// <param name="messageId">The id of the message.</param>
    /// <param name="referer">The referer.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkMessageAsDone(long messageId, string referer, string tenantIdentifier);
}
