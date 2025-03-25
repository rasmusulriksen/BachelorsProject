using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Visma.Ims.NotificationAPI.Model;

public class Notification
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    
    [JsonProperty("postUserId")]
    public string PostUserId { get; set; }
    
    [JsonProperty("feedUserId")]
    public string FeedUserId { get; set; }
    
    [JsonProperty("activityType")]
    public string ActivityType { get; set; }
    
    [JsonProperty("activitySummary")]
    public JObject ActivitySummary { get; set; }
    
    [JsonProperty("isRead")]
    public bool IsRead { get; set; }
    
    [JsonProperty("postDate")]
    public long PostDate { get; set; }
}