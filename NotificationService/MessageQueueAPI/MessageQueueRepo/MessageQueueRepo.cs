// <copyright file="MessageQueueRepo.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.Common.Infrastructure.Tenant;
using Visma.Ims.Common.Abstractions.Queues;

/// <summary>
/// Represents the message queue repository.
/// </summary>
public class MessageQueueRepo : IMessageQueueRepo
{
    private readonly string connectionString;
    private readonly Dictionary<string, IQueueInserter<EventNameAndMessage>> queueInserters = new Dictionary<string, IQueueInserter<EventNameAndMessage>>();
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
    /// Enqueues a message to the message queue.
    /// </summary>
    /// <param name="jsonString">The JSON string to publish.</param>
    /// <param name="eventName">The event name.</param>
    /// <returns>The id of the inserted message.</returns>
    public async Task<long> EnqueueMessage(string jsonString, string eventName)
    {
        string queueName = EventNameToDbTableMapper.GetDbTableForEventName(eventName);

        if (!this.queueInserters.TryGetValue(queueName, out IQueueInserter<EventNameAndMessage>? queueInserter))
        {
            TenantDatabaseConnectionInfoDto connectionInfo = BuildDbConnection(this.connectionString);

            queueInserter = new QueueInserter(queueName, connectionInfo, this.logger);
            this.queueInserters[queueName] = queueInserter;
        }

        var messageContent = new EventNameAndMessage
        {
            Content = jsonString,
            EventName = eventName
        };

        var ids = await queueInserter.Insert(new[] { messageContent }).ConfigureAwait(false);
        return ids.First();
    }

    /// <summary>
    /// Dequeues messages from the message queue.
    /// </summary>
    /// <param name="callingProcessorId">The id of the calling processor.</param>
    /// <param name="numElements">The number of messages to dequeue.</param>
    /// <param name="queueTableName">The name of the queue table.</param>
    /// <returns>The list of dequeued messages.</returns>
    public async Task<List<IdAndMessage>> DequeueMessages(string callingProcessorId, int numElements, string queueTableName)
    {
        List<IdAndMessage> messages = new List<IdAndMessage>();
        using (var connection = new NpgsqlConnection(this.connectionString))
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT * FROM queues.take_elements_for_processing(@queueName, @callingProcessorId, @numElements)",
                connection))
            {
                command.Parameters.AddWithValue("queueName", queueTableName);
                command.Parameters.AddWithValue("callingProcessorId", callingProcessorId);
                command.Parameters.AddWithValue("numElements", numElements);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var id = reader.GetInt64(0); // First column is id (index 0)
                        var messageJson = reader.GetString(1); // Second column is message (index 1)

                        messages.Add(new IdAndMessage
                        {
                            Id = id,
                            Message = messageJson
                        });
                    }
                }
            }
        }

        return messages;
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
        using (var connection = new NpgsqlConnection(this.connectionString))
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand("SELECT queues.mark_element_as_done(@queueName, @messageId, @processingResultText, @callingProcessorId)", connection))
            {
                // Add parameters for the function
                command.Parameters.AddWithValue("queueName", queueName);
                command.Parameters.AddWithValue("messageId", messageId);
                command.Parameters.AddWithValue("processingResultText", processingResultText);
                command.Parameters.AddWithValue("callingProcessorId", callingProcessorId);

                // Execute the command
                await command.ExecuteNonQueryAsync();
            }
        }
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
