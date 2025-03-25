using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Visma.Ims.NotificationAPI.Configuration;

public class ConnectionStringFactory
{
    private readonly string baseConnectionString;

    public ConnectionStringFactory(IConfiguration configuration)
    {
        // Get the base connection string without the database name
        var builder = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("ConnectionString"));
        builder.Database = "postgres"; // Use postgres as the default database
        this.baseConnectionString = builder.ConnectionString;
    }

    public string CreateConnectionString(string tenantIdentifier)
    {
        var builder = new NpgsqlConnectionStringBuilder(this.baseConnectionString);
        builder.Database = tenantIdentifier;
        return builder.ConnectionString;
    }
} 