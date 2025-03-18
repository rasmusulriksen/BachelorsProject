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
    private readonly Dictionary<string, IQueueInserter<JObject>> queueInserters = new Dictionary<string, IQueueInserter<JObject>>();
    private readonly Dictionary<string, IQueueProcessor<JObject>> queueProcessors = new Dictionary<string, IQueueProcessor<JObject>>();
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
    /// <param name="message">The message to publish as a JSON object.</param>
    /// <param name="eventName">The event name.</param>
    /// <returns>The id of the inserted message.</returns>
    public async Task<long> EnqueueMessage(JObject message, string eventName)
    {
        string queueName = EventNameToDbTableMapper.GetDbTableForEventName(eventName);

        if (!this.queueInserters.TryGetValue(queueName, out IQueueInserter<JObject>? queueInserter))
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
    /// <param name="referer">The referer.</param>
    /// <param name="count">The number of messages to dequeue.</param>
    /// <returns>The enumeration of dequeued messages.</returns>
    public async Task<IEnumerable<IdAndJObject>> DequeueMessages(string referer, int count)
    {
        string queueName = RefererToQueueTableMapper.GetQueueTableName(referer);

        if (!this.queueProcessors.TryGetValue(queueName, out IQueueProcessor<JObject>? queueProcessor))
        {
            TenantDatabaseConnectionInfoDto connectionInfo = BuildDbConnection(this.connectionString);
            queueProcessor = new QueueProcessor(queueName, connectionInfo, this.logger);
            this.queueProcessors[queueName] = queueProcessor;
        }

        IEnumerable<(long Id, JObject Element)> results = await queueProcessor.TakeElementsForProcessing(count).ConfigureAwait(false);

        // Map results to custom object that we want to return to the controller
        IEnumerable<IdAndJObject> messagesToReturn = results.Select(r => new IdAndJObject { Id = r.Id, JObject = r.Element });

        return messagesToReturn;
    }

    /// <summary>
    /// Marks a message as done.
    /// </summary>
    /// <param name="messageId">The id of the message.</param>
    /// <param name="referer">The referer.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task MarkMessageAsDone(long messageId, string referer)
    {
        string queueName = RefererToQueueTableMapper.GetQueueTableName(referer);

        if (!this.queueProcessors.TryGetValue(queueName, out IQueueProcessor<JObject>? queueProcessor))
        {
            TenantDatabaseConnectionInfoDto connectionInfo = BuildDbConnection(this.connectionString);
            queueProcessor = new QueueProcessor(queueName, connectionInfo, this.logger);
            this.queueProcessors[queueName] = queueProcessor;
        }

        await queueProcessor.MakeElementDone(messageId, "Success").ConfigureAwait(false);
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
