using System.Text.Json;

namespace NotificationAPI.Model;

public class InboundNotification
{
    public string ActivityType { get; set; }
    public JsonDocument JsonData { get; set; }
    public string UserName { get; set; }
    public string ToEmail { get; set; }
    public string FromEmail { get; set; }

    public InboundNotification() { }
}
