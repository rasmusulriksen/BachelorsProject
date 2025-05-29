// <copyright file="TenantControlPanelService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.TenantControlPanel;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Visma.Ims.Common.Infrastructure.Logging;

public class TenantControlPanelService
{
    private readonly string tenantControlPanelConnectionString;
    private readonly string tenantsDatabaseConnectionString;
    private readonly ILogFactory logger;
    private readonly IConfiguration configuration;

    public TenantControlPanelService(IConfiguration configuration, ILogFactory logger)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.tenantControlPanelConnectionString = configuration.GetConnectionString("TenantControlPanelConnectionString");
        this.tenantsDatabaseConnectionString = configuration.GetConnectionString("TenantsDatabaseConnectionString");
    }

    public async Task OnboardTenant(OnboardTenantRequest request)
    {
        try
        {
            this.logger.Log().Information($"Starting tenant onboarding process for tenant: {request.TenantIdentifier}");

            // Generate a secure password for the tenant database user
            string tenantDbPassword = GenerateSecurePassword(24);

            // Create a new tenant in the tenantcontrolpanel database with connection string
            await CreateTenantRecord(request, tenantDbPassword);
            this.logger.Log().Information($"Created tenant record in tenantcontrolpanel database for tenant: {request.TenantIdentifier}");

            // 2. Create a new database for the tenant
            await CreateTenantDatabase(request.TenantIdentifier);
            this.logger.Log().Information($"Created new database for tenant: {request.TenantIdentifier}");

            // Create connection string for the new tenant database
            var builder = new NpgsqlConnectionStringBuilder(tenantsDatabaseConnectionString);
            builder.Database = request.TenantIdentifier;
            var tenantConnectionString = builder.ConnectionString;

            // 3. Create schema "notification" in the new database
            // 4. Create table "notification.notification_preferences" and "notification.notification" in the new database
            await ExecuteSqlScriptOnTenantDatabaseWithQuotedTenantIdentifier(tenantConnectionString, "Onboard/create_notification_schema.sql", request.TenantIdentifier);
            this.logger.Log().Information($"Created notification schema and tables for tenant: {request.TenantIdentifier}");

            // 5. Create schema "queues" in the new database
            // 6. Create queue tables and functions in the new database
            await ExecuteSqlScriptOnTenantDatabaseWithoutQuotedTenantIdentifier(tenantConnectionString, "Onboard/create_queues_schema.sql", request.TenantIdentifier);
            this.logger.Log().Information($"Created queues schema and tables for tenant: {request.TenantIdentifier}");

            // 7. Create tenant-specific database user
            await CreateTenantDatabaseUser(tenantConnectionString, request.TenantIdentifier, tenantDbPassword);
            this.logger.Log().Information($"Created database user for tenant: {request.TenantIdentifier}");

            this.logger.Log().Information($"Successfully completed tenant onboarding process for tenant: {request.TenantIdentifier}");
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, $"Error onboarding tenant {request.TenantIdentifier}: {ex.Message}");
            throw;
        }
    }

    public async Task TeardownTenant(string tenantIdentifier)
    {
        try
        {
            this.logger.Log().Information($"Starting tenant teardown process for tenant: {tenantIdentifier}");

            // 1. Delete the tenant record from the tenant control panel database first
            await ExecuteSqlScriptOnTenantDatabaseWithQuotedTenantIdentifier(tenantControlPanelConnectionString, "Teardown/teardown_tenant_record.sql", tenantIdentifier);
            this.logger.Log().Information($"Deleted tenant record for tenant: {tenantIdentifier}");


            // 2. Drop the tenant's database - this needs to be done in a separate connection without a transaction
            using var connection = new NpgsqlConnection(tenantsDatabaseConnectionString);
            await connection.OpenAsync();

            // First terminate all connections to the database
            using (var terminateCommand = new NpgsqlCommand(
                "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = @dbname AND pid <> pg_backend_pid();",
                connection))
            {
                terminateCommand.Parameters.AddWithValue("dbname", tenantIdentifier);
                await terminateCommand.ExecuteNonQueryAsync();
            }

            // Then drop the database
            using (var dropCommand = new NpgsqlCommand($"DROP DATABASE IF EXISTS {tenantIdentifier};", connection))
            {
                await dropCommand.ExecuteNonQueryAsync();
            }

            // 3. Drop all tenant-specific roles from the PostgreSQL server
            // This needs to be done on the server level database (postgres) or the tenants database
            await ExecuteSqlScriptOnTenantDatabaseWithoutQuotedTenantIdentifier(tenantsDatabaseConnectionString, "Teardown/teardown_tenant_roles.sql", tenantIdentifier);

            this.logger.Log().Information($"Dropped all roles for tenant: {tenantIdentifier}");

            this.logger.Log().Information($"Dropped database for tenant: {tenantIdentifier}");
            this.logger.Log().Information($"Successfully completed tenant teardown process for tenant: {tenantIdentifier}");
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, $"Error tearing down tenant {tenantIdentifier}: {ex.Message}");
            throw;
        }
    }

    private async Task CreateTenantRecord(OnboardTenantRequest request, string tenantDbPassword)
    {
        // Read the SQL script
        string sqlScript = await File.ReadAllTextAsync(Path.Combine("SQL", "Onboard/create_tenant.sql"));

        // Build the tenant-specific connection string
        var builder = new NpgsqlConnectionStringBuilder(tenantsDatabaseConnectionString)
        {
            Database = request.TenantIdentifier,
            Username = $"{request.TenantIdentifier}_user",
            Password = tenantDbPassword
        };

        // Escape single quotes in the connection string for SQL
        string escapedConnectionString = builder.ConnectionString.Replace("'", "''");

        sqlScript = sqlScript
            .Replace("@TenantIdentifier", $"'{request.TenantIdentifier}'")
            .Replace("@TenantName", $"'{request.TenantName}'")
            .Replace("@TenantTier", $"'{request.TenantTier}'")
            .Replace("@DatabaseConnectionString", $"'{escapedConnectionString}'");

        // Execute the script on the tenant control panel database
        using var connection = new NpgsqlConnection(tenantControlPanelConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sqlScript, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task CreateTenantDatabase(string tenantIdentifier)
    {
        using var connection = new NpgsqlConnection(tenantsDatabaseConnectionString);
        await connection.OpenAsync();

        // Check if database already exists
        bool dbExists;
        using (var checkCommand = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @name)", connection))
        {
            checkCommand.Parameters.AddWithValue("name", tenantIdentifier);
            dbExists = (bool)await checkCommand.ExecuteScalarAsync();
        }

        if (!dbExists)
        {
            // Create new database for tenant
            using var createCommand = new NpgsqlCommand($"CREATE DATABASE {tenantIdentifier} WITH OWNER = admin;", connection);
            await createCommand.ExecuteNonQueryAsync();
        }
    }

    // When the tenantIdentifier is used to reference database values (for instance in a WHERE clause), this method is used
    private async Task ExecuteSqlScriptOnTenantDatabaseWithQuotedTenantIdentifier(string connectionString, string scriptName, string tenantIdentifier)
    {
        // Read the SQL script
        string sqlScript = await File.ReadAllTextAsync(Path.Combine("SQL", scriptName));

        sqlScript = sqlScript.Replace("@TenantIdentifier", $"'{tenantIdentifier}'");

        // Execute the script on the tenant database
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sqlScript, connection);
        await command.ExecuteNonQueryAsync();
    }

    // When the tenantIdentifier is merged into db function names for onboarding, this method is used
    private async Task ExecuteSqlScriptOnTenantDatabaseWithoutQuotedTenantIdentifier(string connectionString, string scriptName, string tenantIdentifier)
    {
        // Read the SQL script
        string sqlScript = await File.ReadAllTextAsync(Path.Combine("SQL", scriptName));

        sqlScript = sqlScript.Replace("@TenantIdentifier", tenantIdentifier);

        // Execute the script on the tenant database
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sqlScript, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task CreateTenantDatabaseUser(string connectionString, string tenantIdentifier, string password)
    {
        // Read the SQL script
        string sqlScript = await File.ReadAllTextAsync(Path.Combine("SQL", "Onboard/create_tenant_db_user.sql"));

        sqlScript = sqlScript
            .Replace("@TenantIdentifier", tenantIdentifier)
            .Replace("@TenantPassword", password);

        // Execute the script on the tenant database
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sqlScript, connection);
        await command.ExecuteNonQueryAsync();
    }

    private string GenerateSecurePassword(int length)
    {
        // For now, just return "admin"
        // TODO: Make this secure in a production scenario
        return "admin";
    }

    /// <summary>
    /// Retrieves the database connection string for a specific tenant
    /// </summary>
    /// <param name="tenantIdentifier">The unique identifier for the tenant</param>
    /// <returns>The database connection string for the tenant</returns>
    public async Task<string> GetTenantConnectionString(string tenantIdentifier)
    {
        try
        {
            this.logger.Log().Information($"Retrieving database connection string for tenant: {tenantIdentifier}");

            using var connection = new NpgsqlConnection(tenantControlPanelConnectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT database_connectionstring FROM tenant.tenant WHERE tenant_identifier = @tenant_id;",
                connection);
            command.Parameters.AddWithValue("tenant_id", tenantIdentifier);

            var result = await command.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
            {
                throw new KeyNotFoundException($"No tenant found with identifier: {tenantIdentifier}");
            }

            this.logger.Log().Information($"Successfully retrieved connection string for tenant: {tenantIdentifier}");
            return result.ToString();
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, $"Error retrieving connection string for tenant: {tenantIdentifier}");
            throw new InvalidOperationException($"Failed to retrieve connection string for tenant: {tenantIdentifier}", ex);
        }
    }
}
