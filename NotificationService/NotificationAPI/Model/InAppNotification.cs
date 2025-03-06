using System.Text.Json.Serialization;

namespace Model;

public class InAppNotification
    {
        [JsonPropertyName("activityType")]
        public string ActivityType { get; set; }
        
        [JsonPropertyName("jsonData")]
        public JsonData JsonData { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }
    }