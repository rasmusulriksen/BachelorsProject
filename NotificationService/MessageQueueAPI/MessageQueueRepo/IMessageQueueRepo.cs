// <copyright file="IMessageQueueRepo.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

using System.Threading.Tasks;
using System.Collections.Generic;
using Model;

/// <summary>
/// Interface for the message queue repository.
/// </summary>
public interface IMessageQueueRepo
{
    /// <summary>
    /// Enqueues a message to the message queue.
    /// </summary>
    /// <param name="jsonString">The JSON string to publish.</param>
    /// <param name="eventName">The event name.</param>
    /// <returns>The id of the inserted message.</returns>
    Task<long> EnqueueMessage(string jsonString, string eventName);

    /// <summary>
    /// Dequeues messages from the message queue.
    /// </summary>
    /// <param name="callingProcessorId">The id of the calling processor.</param>
    /// <param name="numElements">The number of messages to dequeue.</param>
    /// <param name="queueTableName">The name of the queue table.</param>
    /// <returns>The list of dequeued messages.</returns>
    Task<List<QueueMessage>> DequeueMessages(string callingProcessorId, int numElements, string queueTableName);
}
