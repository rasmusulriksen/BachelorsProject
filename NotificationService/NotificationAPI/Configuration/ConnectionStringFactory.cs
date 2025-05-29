// <copyright file="ConnectionStringFactory.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Configuration;

using Microsoft.Extensions.Configuration;
using Npgsql;

public class ConnectionStringFactory
{
    private readonly string baseConnectionString;

    public ConnectionStringFactory(IConfiguration configuration)
    {
        this.baseConnectionString = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("ConnectionString")).ConnectionString;
    }

    public string CreateConnectionString(string tenantIdentifier)
    {
        var builder = new NpgsqlConnectionStringBuilder(this.baseConnectionString);
        builder.Database = tenantIdentifier;        
        return builder.ConnectionString;
    }
} 