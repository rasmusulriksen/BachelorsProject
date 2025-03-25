using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Visma.Ims.NotificationAPI.Model;

namespace Visma.Ims.NotificationAPI.Model;

public class MyNotificationsResponse
{
    [JsonProperty("totalItems")]
    public long TotalItems { get; set; }
    
    [JsonProperty("activities")]
    public List<Notification> Activities { get; set; }
    
    [JsonProperty("totalPages")]
    public long TotalPages { get; set; }
} 