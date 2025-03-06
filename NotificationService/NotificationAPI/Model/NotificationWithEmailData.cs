using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotificationAPI.Model;

public class NotificationWithEmailData
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

    public NotificationWithEmailData(string activityType, JsonData jsonData, string userName, string toEmail, string fromEmail)
    {
        ActivityType = activityType;
        JsonData = jsonData;
        UserName = userName;
        ToEmail = toEmail;
        FromEmail = fromEmail;
    }

    public NotificationWithEmailData() { }
}
