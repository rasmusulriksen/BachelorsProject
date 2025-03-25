namespace Visma.Ims.NotificationAPI.Repositories;

using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Visma.Ims.NotificationAPI.Model;

/// <summary>
/// PostgreSQL implementation of notification preference repository.
/// </summary>
public class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPreferencesRepository"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public NotificationPreferencesRepository(IConfiguration configuration)
    {
        this.connectionString = configuration.GetConnectionString("ConnectionString");
    }

    /// <inheritdoc/>
    public async Task<NotificationPreference> GetByUsernameAsync(string username)
    {
        using var connection = new NpgsqlConnection(this.connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(
            "SELECT username, case_owner, email_enabled, in_app_enabled, links_enabled, created_at, updated_at " +
            "FROM notification.notification_preferences " +
            "WHERE username = @username",
            connection);

        command.Parameters.AddWithValue("@username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new NotificationPreference
            {
                UserName = reader.GetString(0),
                CaseOwner = reader.GetBoolean(1),
                EmailEnabled = reader.GetBoolean(2),
                InAppEnabled = reader.GetBoolean(3),
                LinksEnabled = reader.GetBoolean(4),
            };
        }

        return null;
    }
}