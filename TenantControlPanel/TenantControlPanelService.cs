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
using Visma.Ims.Common.Abstractions.Logging;

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

            // 1. Create a new tenant in the tenantcontrolpanel database
            await CreateTenantRecord(request);
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

            this.logger.Log().Information($"Dropped database for tenant: {tenantIdentifier}");
            this.logger.Log().Information($"Successfully completed tenant teardown process for tenant: {tenantIdentifier}");
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, $"Error tearing down tenant {tenantIdentifier}: {ex.Message}");
            throw;
        }
    }

    private async Task CreateTenantRecord(OnboardTenantRequest request)
    {
        // Read the SQL script
        string sqlScript = await File.ReadAllTextAsync(Path.Combine("SQL", "Onboard/create_tenant.sql"));

        sqlScript = sqlScript
            .Replace("@TenantIdentifier", $"'{request.TenantIdentifier}'")
            .Replace("@TenantName", $"'{request.TenantName}'")
            .Replace("@TenantTier", $"'{request.TenantTier}'");

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
            // Note: We're using parameterless command for DDL statements since
            // Postgres doesn't support parameters for database names
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
}
