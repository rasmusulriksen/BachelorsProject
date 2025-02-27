using System.Text.Json;
using System.Text.Json.Serialization;

public class NotificationWithoutEmailData
{
    [JsonPropertyName("notificationType")]
    public string NotificationType { get; set; }

    [JsonPropertyName("jsonData")]
    public string JsonData { get; set; }
    
    [JsonPropertyName("userName")] // Ensure this matches what the Java endpoint expects
    public string UserName { get; set; }

    public NotificationWithoutEmailData(string notificationType, string jsonData, string userName)
    {
        NotificationType = notificationType;
        JsonData = jsonData;
        UserName = userName;
    }
}