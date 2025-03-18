// <copyright file="MessageQueueRepo.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Newtonsoft.Json.Linq;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.Common.Abstractions.Queues;
using Visma.Ims.Common.Infrastructure.Queues;
using Visma.Ims.Common.Infrastructure.Tenant;
using Visma.Ims.NotificationService.MessageQueueAPI.Model;

/// <summary>
/// Represents the message queue repository.
/// </summary>
public class MessageQueueRepo : IMessageQueueRepo
{
    private readonly string connectionString;
    private readonly Dictionary<string, IQueueInserter<NotificationMessage>> queueInserters = new Dictionary<string, IQueueInserter<NotificationMessage>>();
    private readonly Dictionary<string, IQueueProcessor<JToken>> queueProcessors = new Dictionary<string, IQueueProcessor<JToken>>();
    private readonly ILogFactory logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageQueueRepo"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the database.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public MessageQueueRepo(string connectionString, ILogFactory logger)
    {
        this.connectionString = connectionString;
        this.logger = logger;
    }

    /// <summary>
    /// Enqueues a message into the message queue.
    /// </summary>
    /// <param name="message">The message object to publish.</param>
    /// <param name="eventName">The event name.</param>
    /// <returns>The id of the inserted message.</returns>
    public async Task<long> EnqueueMessage(NotificationMessage message, string eventName)
    {
        string queueName = EventNameToDbTableMapper.GetDbTableForEventName(eventName);

        if (!this.queueInserters.TryGetValue(queueName, out IQueueInserter<NotificationMessage>? queueInserter))
        {
            TenantDatabaseConnectionInfoDto connectionInfo = BuildDbConnection(this.connectionString);
            queueInserter = new QueueInserter(queueName, connectionInfo, this.logger);
            this.queueInserters[queueName] = queueInserter;
        }

        var ids = await queueInserter.Insert(new[] { message }).ConfigureAwait(false);  
        return ids.First();
    }

    /// <summary>
    /// Dequeues messages from the message queue.
    /// </summary>
    /// <param name="callingProcessorId">The id of the calling processor.</param>
    /// <param name="numElements">The number of messages to dequeue.</param>
    /// <param name="queueTableName">The name of the queue table.</param>
    /// <returns>The enumeration of dequeued messages.</returns>
    public async Task<IEnumerable<IdAndMessage>> DequeueMessages(string callingProcessorId, int numElements, string queueTableName)
    {
        if (!this.queueProcessors.TryGetValue(queueTableName, out IQueueProcessor<JToken>? queueProcessor))
        {
            TenantDatabaseConnectionInfoDto connectionInfo = BuildDbConnection(this.connectionString);
            queueProcessor = new QueueProcessor(queueTableName, connectionInfo, this.logger);
            this.queueProcessors[queueTableName] = queueProcessor;
        }

        IEnumerable<(long Id, JToken Element)> results = await queueProcessor.TakeElementsForProcessing(numElements).ConfigureAwait(false);

        return results.Select(result =>
        {
            string jsonMessage = result.Element.ToString();

            return new IdAndMessage
            {
                Id = result.Id,
                Message = jsonMessage
            };
        });
    }

    /// <summary>
    /// Marks a message as done.
    /// </summary>
    /// <param name="messageId">The id of the message.</param>
    /// <param name="processingResultText">The processing result text.</param>
    /// <param name="callingProcessorId">The id of the calling processor.</param>
    /// <param name="queueName">The name of the queue.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task MarkMessageAsDone(long messageId, string processingResultText, string callingProcessorId, string queueName)
    {
        if (!this.queueProcessors.TryGetValue(queueName, out IQueueProcessor<JToken>? queueProcessor))
        {
            TenantDatabaseConnectionInfoDto connectionInfo = BuildDbConnection(this.connectionString);
            queueProcessor = new QueueProcessor(queueName, connectionInfo, this.logger);
            this.queueProcessors[queueName] = queueProcessor;
        }

        await queueProcessor.MakeElementDone(messageId, processingResultText).ConfigureAwait(false);
    }

    private static TenantDatabaseConnectionInfoDto BuildDbConnection(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrEmpty(builder.Host) ||
            string.IsNullOrEmpty(builder.Database) ||
            string.IsNullOrEmpty(builder.Username) ||
            string.IsNullOrEmpty(builder.Password))
        {
            throw new ArgumentException("Connection string is missing required components", nameof(connectionString));
        }

        return new TenantDatabaseConnectionInfoDto
        {
            Host = builder.Host,
            Port = builder.Port.ToString(),
            Database = builder.Database,
            User = builder.Username,
            Password = builder.Password
        };
    }
}
