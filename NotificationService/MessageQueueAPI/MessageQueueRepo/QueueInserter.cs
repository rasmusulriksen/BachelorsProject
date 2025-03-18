// <copyright file="QueueInserter.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.Common.Infrastructure.Queues;
using Visma.Ims.Common.Infrastructure.Tenant;

/// <summary>
/// Represents a queue inserter for notification service messages.
/// </summary>
public class QueueInserter : QueueInserterBase<NotificationMessage>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueueInserter"/> class.
    /// </summary>
    /// <param name="queueName">Name of the queue this class inserts into.</param>
    /// <param name="connectionInfoDto">Connection information to the database.</param>
    /// <param name="logger">The logger used to log.</param>
    public QueueInserter(
        string queueName,
        TenantDatabaseConnectionInfoDto connectionInfoDto,
        ILogFactory logger)
        : base(queueName, connectionInfoDto, logger)
    {
    }
}
