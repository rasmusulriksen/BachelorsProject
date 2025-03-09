namespace NotificationAPI.Model;

public class NotificationPreference
{
    public string Email { get; set; }
    public bool EmailEnabled { get; set; }
    public bool InAppEnabled { get; set; }
    public bool LinksEnabled { get; set; }
} 