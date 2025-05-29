// <copyright file="NotificationRepository.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Repositories;

using System.Data;
using Npgsql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Visma.Ims.Common.Infrastructure.Logging;
using Visma.Ims.NotificationAPI.Configuration;
using Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Repository for notification data access.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly ConnectionStringFactory connectionStringFactory;
    private readonly ILogFactory logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="connectionStringFactory">The connection string factory.</param>
    public NotificationRepository(ILogFactory logger, ConnectionStringFactory connectionStringFactory)
    {
        this.logger = logger;
        this.connectionStringFactory = connectionStringFactory;
    }

    /// <inheritdoc/>
    public async Task<Notification> GetByIdAsync(Guid id, string tenantIdentifier)
    {
        using var connection = new NpgsqlConnection(this.connectionStringFactory.CreateConnectionString(tenantIdentifier));
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(
            "SELECT id, post_user_id, feed_user_id, activity_type, activity_summary, is_read, post_date " +
            "FROM notification.notification WHERE id = @Id", connection);

        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return this.ReadNotification(reader);
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<MyNotificationsResponse> GetForUserAsync(string userName, int page, int pageSize, string tenantIdentifier)
    {
        using var connection = new NpgsqlConnection(this.connectionStringFactory.CreateConnectionString(tenantIdentifier));
        await connection.OpenAsync();

        // Get total count
        long totalItems;
        using (var countCommand = new NpgsqlCommand(
            "SELECT COUNT(*) FROM notification.notification WHERE feed_user_id = @UserId", connection))
        {
            countCommand.Parameters.AddWithValue("@UserId", userName);
            totalItems = Convert.ToInt64(await countCommand.ExecuteScalarAsync());
        }

        var totalPages = (long)Math.Ceiling((double)totalItems / pageSize);

        // Get paged results
        var offset = (page - 1) * pageSize;
        var notifications = new List<Notification>();

        using (var command = new NpgsqlCommand(
            @"SELECT id, post_user_id, feed_user_id, activity_type, activity_summary, is_read, post_date 
              FROM notification.notification 
              WHERE feed_user_id = @UserId 
              ORDER BY post_date DESC
              LIMIT @PageSize OFFSET @Offset", connection))
        {
            command.Parameters.AddWithValue("@UserId", userName);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            command.Parameters.AddWithValue("@Offset", offset);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                notifications.Add(this.ReadNotification(reader));
            }
        }

        return new MyNotificationsResponse
        {
            TotalItems = totalItems,
            Activities = notifications,
            TotalPages = totalPages
        };
    }

    /// <inheritdoc/>
    public async Task CreateAsync(Notification notification, string tenantIdentifier)
    {
        using var connection = new NpgsqlConnection(this.connectionStringFactory.CreateConnectionString(tenantIdentifier));
        await connection.OpenAsync();

        // If it's a new notification, generate a new GUID
        if (notification.Id == Guid.Empty)
        {
            notification.Id = Guid.NewGuid();
        }

        // Convert JObject to string for storage
        var activitySummaryJson = notification.ActivitySummary.ToString(Formatting.None);

        using var command = new NpgsqlCommand(
            @"INSERT INTO notification.notification (id, post_user_id, feed_user_id, activity_type, activity_summary, is_read, post_date)
              VALUES (@Id, @PostUserId, @FeedUserId, @ActivityType, @ActivitySummary, @IsRead, @PostDate)
              RETURNING id", connection);

        command.Parameters.AddWithValue("@Id", notification.Id);
        command.Parameters.AddWithValue("@PostUserId", notification.PostUserId);
        command.Parameters.AddWithValue("@FeedUserId", notification.FeedUserId);
        command.Parameters.AddWithValue("@ActivityType", notification.ActivityType);
        command.Parameters.AddWithValue("@ActivitySummary", activitySummaryJson);
        command.Parameters.AddWithValue("@IsRead", notification.IsRead);
        command.Parameters.AddWithValue("@PostDate", notification.PostDate);

        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<Notification> UpdateAsync(Notification notification, string tenantIdentifier)
    {
        using var connection = new NpgsqlConnection(this.connectionStringFactory.CreateConnectionString(tenantIdentifier));
        await connection.OpenAsync();

        // Convert JObject to string for storage
        var activitySummaryJson = notification.ActivitySummary.ToString(Formatting.None);

        using var command = new NpgsqlCommand(
            @"UPDATE notification.notification
              SET post_user_id = @PostUserId,
                  feed_user_id = @FeedUserId,
                  activity_type = @ActivityType,
                  activity_summary = @ActivitySummary,
                  is_read = @IsRead,
                  post_date = @PostDate
              WHERE id = @Id", connection);

        command.Parameters.AddWithValue("@Id", notification.Id);
        command.Parameters.AddWithValue("@PostUserId", notification.PostUserId);
        command.Parameters.AddWithValue("@FeedUserId", notification.FeedUserId);
        command.Parameters.AddWithValue("@ActivityType", notification.ActivityType);
        command.Parameters.AddWithValue("@ActivitySummary", activitySummaryJson);
        command.Parameters.AddWithValue("@IsRead", notification.IsRead);
        command.Parameters.AddWithValue("@PostDate", notification.PostDate);

        await command.ExecuteNonQueryAsync();
        return notification;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, string tenantIdentifier)
    {
        using var connection = new NpgsqlConnection(this.connectionStringFactory.CreateConnectionString(tenantIdentifier));
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(
            "DELETE FROM notification.notification WHERE id = @Id", connection);

        command.Parameters.AddWithValue("@Id", id);
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task MarkAsReadAsync(Guid id, string tenantIdentifier)
    {
        using var connection = new NpgsqlConnection(this.connectionStringFactory.CreateConnectionString(tenantIdentifier));
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(
            "UPDATE notification.notification SET is_read = true WHERE id = @Id", connection);

        command.Parameters.AddWithValue("@Id", id);
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<long> GetUnreadCountForUserAsync(string userId, string tenantIdentifier)
    {
        using var connection = new NpgsqlConnection(this.connectionStringFactory.CreateConnectionString(tenantIdentifier));
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM notification.notification WHERE feed_user_id = @UserId AND is_read = false",
            connection);

        command.Parameters.AddWithValue("@UserId", userId);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    /// <inheritdoc/>
    public async Task<long> GetTotalCountForUserAsync(string userId, string tenantIdentifier)
    {
        using var connection = new NpgsqlConnection(this.connectionStringFactory.CreateConnectionString(tenantIdentifier));
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM notification.notification WHERE feed_user_id = @UserId",
            connection);

        command.Parameters.AddWithValue("@UserId", userId);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private Notification ReadNotification(NpgsqlDataReader reader)
    {
        var notification = new Notification
        {
            Id = reader.GetGuid(0),
            PostUserId = reader.GetString(1),
            FeedUserId = reader.GetString(2),
            ActivityType = reader.GetString(3),
            ActivitySummary = JObject.Parse(reader.GetString(4)),
            IsRead = reader.GetBoolean(5),
            PostDate = reader.GetInt64(6)
        };

        return notification;
    }
}
