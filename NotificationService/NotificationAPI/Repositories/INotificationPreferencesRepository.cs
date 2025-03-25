namespace Visma.Ims.NotificationAPI.Repositories;

using Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Interface for notification preference repository operations.
/// </summary>
public interface INotificationPreferencesRepository
{
    /// <summary>
    /// Gets notification preferences for a specific user.
    /// </summary>
    /// <param name="username">The username to retrieve preferences for.</param>
    /// <returns>The notification preferences or null if not found.</returns>
    Task<NotificationPreference> GetByUsernameAsync(string username);
} 