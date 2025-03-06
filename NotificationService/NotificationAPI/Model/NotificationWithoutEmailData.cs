using System.Text.Json;
using System.Text.Json.Serialization;

public class NotificationWithoutEmailData
{
    [JsonPropertyName("activityType")]
    public string ActivityType { get; set; }

    [JsonPropertyName("jsonData")]
    public string JsonData { get; set; }
    
    [JsonPropertyName("userName")] // Ensure this matches what the Java endpoint expects
    public string UserName { get; set; }

    public NotificationWithoutEmailData(string activityType, string jsonData, string userName)
    {
        ActivityType = activityType;
        JsonData = jsonData;
        UserName = userName;
    }
}