// <copyright file="QueueProcessor.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI
{
    using Newtonsoft.Json.Linq;
    using Visma.Ims.Common.Abstractions.Logging;
    using Visma.Ims.Common.Infrastructure.Queues;
    using Visma.Ims.Common.Infrastructure.Tenant;

    /// <summary>
    /// Represents a queue processor for notification service messages.
    /// </summary>
    public class QueueProcessor : QueueProcessorBase<JObject>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueProcessor"/> class.
        /// </summary>
        /// <param name="queueName">Name of the queue this class processes.</param>
        /// <param name="connectionInfoDto">Connection information to the database.</param>
        /// <param name="logger">The logger used to log.</param>
        public QueueProcessor(
            string queueName,
            TenantDatabaseConnectionInfoDto connectionInfoDto,
            ILogFactory logger)
            : base(queueName, connectionInfoDto, logger)
        {
        }
    }
}
