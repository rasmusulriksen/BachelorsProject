// <copyright file="Message.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a message in the message queue.
/// </summary>
public class Message
{
    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    [JsonPropertyName("activityType")]
    required public string ActivityType { get; set; }

    /// <summary>
    /// Gets or sets the JSON data.
    /// </summary>
    [JsonPropertyName("jsonData")]
    required public JsonData JsonData { get; set; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    [JsonPropertyName("userName")]
    required public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the to email.
    /// </summary>
    [JsonPropertyName("toEmail")]
    required public string ToEmail { get; set; }

    /// <summary>
    /// Gets or sets the from email.
    /// </summary>
    [JsonPropertyName("fromEmail")]
    required public string FromEmail { get; set; }
}
