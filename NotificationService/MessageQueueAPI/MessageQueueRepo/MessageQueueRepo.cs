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
using Visma.Ims.Common.Infrastructure.Tenant;
using Visma.Ims.MessageQueueAPI.Configuration;
using Visma.Ims.NotificationService.MessageQueueAPI.Model;

/// <summary>
/// Represents the message queue repository.
/// </summary>
public class MessageQueueRepo : IMessageQueueRepo
{
    private readonly ConnectionStringFactory connectionStringFactory;
    private readonly ILogFactory logger;
    
    // Keep track of the last connection stats check time to avoid checking too frequently
    private DateTime lastConnectionCheckTime = DateTime.MinValue;
    private readonly TimeSpan connectionCheckInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageQueueRepo"/> class.
    /// </summary>
    /// <param name="connectionStringFactory">The connection string factory.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public MessageQueueRepo(ConnectionStringFactory connectionStringFactory, ILogFactory logger)
    {
        this.connectionStringFactory = connectionStringFactory;
        this.logger = logger;
    }

    /// <summary>
    /// Enqueues a message into the message queue.
    /// </summary>
    /// <param name="message">The message to publish as a JSON object.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The id of the inserted message.</returns>
    public async Task<long> EnqueueMessage(JObject message, string eventName, string tenantIdentifier)
    {
        string connectionString = this.connectionStringFactory.CreateConnectionString(tenantIdentifier);
        string queueName = EventNameToDbTableMapper.GetDbTableForEventName(eventName);

        // Check if we need to log connection statistics
        await this.CheckConnectionCountAsync(tenantIdentifier);

        // Create a new inserter for this specific operation rather than caching it
        var connectionInfo = BuildDbConnection(connectionString);
        var queueInserter = new QueueInserter(queueName, connectionInfo, this.logger);
        
        try
        {
            var ids = await queueInserter.Insert(new[] { message }).ConfigureAwait(false);
            return ids.First();
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error inserting message into queue {QueueName} for tenant {TenantIdentifier}", 
                queueName, tenantIdentifier);
            throw;
        }
    }

    /// <summary>
    /// Dequeues messages from the message queue.
    /// </summary>
    /// <param name="referer">The referer.</param>
    /// <param name="count">The number of messages to dequeue.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>The enumeration of dequeued messages.</returns>
    public async Task<IEnumerable<IdAndJObject>> DequeueMessages(string referer, int count, string tenantIdentifier)
    {
        string connectionString = this.connectionStringFactory.CreateConnectionString(tenantIdentifier);
        string queueName = RefererToQueueTableMapper.GetQueueTableName(referer);

        // Check if we need to log connection statistics
        await this.CheckConnectionCountAsync(tenantIdentifier);

        // Create a new processor for this specific operation rather than caching it
        var connectionInfo = BuildDbConnection(connectionString);
        var queueProcessor = new QueueProcessor(queueName, connectionInfo, this.logger);
        
        try
        {
            IEnumerable<(long Id, JObject Element)> results = await queueProcessor.TakeElementsForProcessing(count).ConfigureAwait(false);

            // Map results to custom object that we want to return to the controller
            IEnumerable<IdAndJObject> messagesToReturn = results.Select(r => new IdAndJObject { Id = r.Id, JObject = r.Element });

            return messagesToReturn;
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error dequeuing messages from queue {QueueName} for tenant {TenantIdentifier}", 
                queueName, tenantIdentifier);
            
            throw;
        }
    }

    /// <summary>
    /// Marks a message as done.
    /// </summary>
    /// <param name="messageId">The id of the message.</param>
    /// <param name="referer">The referer.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task MarkMessageAsDone(long messageId, string referer, string tenantIdentifier)
    {
        string connectionString = this.connectionStringFactory.CreateConnectionString(tenantIdentifier);
        string queueName = RefererToQueueTableMapper.GetQueueTableName(referer);

        // Create a new processor for this specific operation
        var connectionInfo = BuildDbConnection(connectionString);
        var queueProcessor = new QueueProcessor(queueName, connectionInfo, this.logger);
        
        try
        {
            await queueProcessor.MakeElementDone(messageId, "Success").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error marking message {MessageId} as done in queue {QueueName} for tenant {TenantIdentifier}", 
                messageId, queueName, tenantIdentifier);
            throw;
        }
    }

    // Periodically check the connection count to help diagnose issues
    private async Task CheckConnectionCountAsync(string tenantIdentifier)
    {
        if (DateTime.UtcNow - lastConnectionCheckTime < connectionCheckInterval)
        {
            return; // Only check once per minute to avoid excessive queries
        }
        
        lastConnectionCheckTime = DateTime.UtcNow;
        
        try
        {
            var connectionString = this.connectionStringFactory.CreateConnectionString(tenantIdentifier);
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            builder.Database = "postgres"; // Use postgres database for admin queries
            
            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync();
                
                // Get total connection count
                using (var cmd = new NpgsqlCommand("SELECT count(*) FROM pg_stat_activity", connection))
                {
                    var count = await cmd.ExecuteScalarAsync();
                    this.logger.Log().Information("Current PostgreSQL connections: {Count}", count);
                }
                
                // Get idle connection count
                using (var cmd = new NpgsqlCommand(
                    "SELECT count(*) FROM pg_stat_activity WHERE state = 'idle'", connection))
                {
                    var count = await cmd.ExecuteScalarAsync();
                    this.logger.Log().Information("Current idle PostgreSQL connections: {Count}", count);
                }
            }
        }
        catch (Exception ex)
        {
            // Just log the error and continue
            this.logger.Log().Warning(ex, "Failed to check connection count");
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
