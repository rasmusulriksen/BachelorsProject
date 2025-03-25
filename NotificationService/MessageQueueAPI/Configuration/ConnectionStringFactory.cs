// <copyright file="ConnectionStringFactory.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.MessageQueueAPI.Configuration;

using Microsoft.Extensions.Configuration;
using Npgsql;

public class ConnectionStringFactory
{
    private readonly string baseConnectionString;

    public ConnectionStringFactory(IConfiguration configuration)
    {
        this.baseConnectionString = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("MessageQueueDb")).ConnectionString;
    }

    public string CreateConnectionString(string tenantIdentifier)
    {
        var builder = new NpgsqlConnectionStringBuilder(this.baseConnectionString);
        builder.Database = tenantIdentifier;

        // Add connection pooling parameters to prevent "too many clients" errors. I dont want these.
        // builder.Pooling = true;
        // builder.MinPoolSize = 1;
        // builder.MaxPoolSize = 5;  // Set a reasonable maximum pool size
        // builder.ConnectionIdleLifetime = 30;  // Close idle connections after 30 seconds

        return builder.ConnectionString;
    }
}