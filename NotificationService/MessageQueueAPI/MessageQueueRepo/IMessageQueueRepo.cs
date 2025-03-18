// <copyright file="IMessageQueueRepo.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

using System.Threading.Tasks;
using System.Collections.Generic;
using Visma.Ims.NotificationService.MessageQueueAPI.Model;

/// <summary>
/// Interface for the message queue repository.
/// </summary>
public interface IMessageQueueRepo
{
    /// <summary>
    /// Enqueues a message to the message queue.
    /// </summary>
    /// <param name="message">The message object to publish.</param>
    /// <param name="eventName">The event name.</param>
    /// <returns>The id of the inserted message.</returns>
    Task<long> EnqueueMessage(NotificationMessage message, string eventName);

    /// <summary>
    /// Dequeues messages from the message queue.
    /// </summary>
    /// <param name="callingProcessorId">The id of the calling processor.</param>
    /// <param name="numElements">The number of messages to dequeue.</param>
    /// <param name="queueTableName">The name of the queue table.</param>
    /// <returns>The list of dequeued messages.</returns>
    Task<IEnumerable<IdAndMessage>> DequeueMessages(string callingProcessorId, int numElements, string queueTableName);

    /// <summary>
    /// Marks a message as done.
    /// </summary>
    /// <param name="messageId">The id of the message.</param>
    /// <param name="processingResultText">The processing result text.</param>
    /// <param name="callingProcessorId">The id of the calling processor.</param>
    /// <param name="queueName">The name of the queue.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkMessageAsDone(long messageId, string processingResultText, string callingProcessorId, string queueName);
}
