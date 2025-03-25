// <copyright file="NotificationFromJavaDto.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// DTO for receiving notification data from Java in the format matching our Notification model.
/// </summary>
public class NotificationFromJavaDto
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the post user id.
    /// </summary>
    [JsonProperty("postUserId")]
    public string PostUserId { get; set; }

    /// <summary>
    /// Gets or sets the feed user id.
    /// </summary>
    [JsonProperty("feedUserId")]
    public string FeedUserId { get; set; }

    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    [JsonProperty("activityType")]
    public string ActivityType { get; set; }

    /// <summary>
    /// Gets or sets the activity summary.
    /// </summary>
    [JsonProperty("activitySummary")]
    public JObject ActivitySummary { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the notification is read.
    /// </summary>
    [JsonProperty("isRead")]
    public bool IsRead { get; set; }

    /// <summary>
    /// Gets or sets the post date.
    /// </summary>
    [JsonProperty("postDate")]
    public long PostDate { get; set; }

    /// <summary>
    /// Gets or sets the to email (for message queue).
    /// </summary>
    [JsonProperty("toEmail")]
    public string ToEmail { get; set; }

    /// <summary>
    /// Gets or sets the from email (for message queue).
    /// </summary>
    [JsonProperty("fromEmail")]
    public string FromEmail { get; set; }

    /// <summary>
    /// Converts this DTO to a Notification entity.
    /// </summary>
    /// <returns>A new Notification entity.</returns>
    public Notification ToNotification()
    {
        return new Notification
        {
            Id = this.Id,
            PostUserId = this.PostUserId,
            FeedUserId = this.FeedUserId,
            ActivityType = this.ActivityType,
            ActivitySummary = this.ActivitySummary,
            IsRead = this.IsRead,
            PostDate = this.PostDate
        };
    }

    /// <summary>
    /// Creates a Message for the message queue from this notification.
    /// </summary>
    /// <returns>A Message for the message queue.</returns>
    public Message ToMessage()
    {
        return new Message
        {
            ActivityType = this.ActivityType,
            JsonData = new JsonData
            {
                ModifierDisplayName = this.ActivitySummary["modifierDisplayName"]?.ToString(),
                CaseId = this.ActivitySummary["caseId"]?.ToString(),
                CaseTitle = this.ActivitySummary["caseTitle"]?.ToString(),
                DocTitle = this.ActivitySummary["docTitle"]?.ToString()
            },
            UserName = this.FeedUserId,
            ToEmail = this.ToEmail,
            FromEmail = this.FromEmail
        };
    }
}
