using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotificationAPI.Model;

public class NotificationFromAlfresco
{
    [JsonPropertyName("activityType")]
    public string ActivityType { get; set; }

    [JsonPropertyName("jsonData")]
    public JsonData JsonData { get; set; }

    [JsonPropertyName("userName")]
    public string UserName { get; set; }

    [JsonPropertyName("toEmail")]
    public string ToEmail { get; set; }

    [JsonPropertyName("fromEmail")]
    public string FromEmail { get; set; }

    [JsonPropertyName("linksEnabled")]
    public bool LinksEnabled { get; set; } = false; // Default to false

    public NotificationFromAlfresco(string activityType, JsonData jsonData, string userName, string toEmail, string fromEmail, bool linksEnabled)
    {
        ActivityType = activityType;
        JsonData = jsonData;
        UserName = userName;
        ToEmail = toEmail;
        FromEmail = fromEmail;
        LinksEnabled = linksEnabled;
    }
}
