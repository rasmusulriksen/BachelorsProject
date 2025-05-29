// <copyright file="Notification.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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