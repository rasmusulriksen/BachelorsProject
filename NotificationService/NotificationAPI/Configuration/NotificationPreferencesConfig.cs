namespace Visma.Ims.NotificationAPI.Configuration;

using Visma.Ims.NotificationAPI.Model;


/// <summary>
/// Configuration for notification preferences.
/// </summary>
public class NotificationPreferencesConfig
{
    /// <summary>
    /// Gets or sets the list of notification preferences.
    /// </summary>
    public List<NotificationPreference> Preferences { get; set; } = new();
} 