// <copyright file="IdAndJObject.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a strongly typed model that is parsed from the raw API response model.
/// </summary>
public class IdAndJObject
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the jObject.
    /// </summary>
    [JsonProperty("jObject")]
    required public JObject JObject { get; set; }

    /// <summary>
    /// Converts the IdAndJObject to an IdAndMessage.
    /// </summary>
    /// <returns>An IdAndMessage.</returns>
    public IdAndMessage ToIdAndMessage()
    {
        return new IdAndMessage
        {
            Id = this.Id,
            Message = new Message
            {
                ActivityType = this.JObject["activityType"]?.ToString() ?? throw new InvalidOperationException("ActivityType is required"),
                JsonData = this.JObject["jsonData"]?.ToObject<JsonData>() ?? throw new InvalidOperationException("JsonData is required"),
                UserName = this.JObject["userName"]?.ToString() ?? throw new InvalidOperationException("UserName is required"),
                ToEmail = this.JObject["toEmail"]?.ToString() ?? throw new InvalidOperationException("ToEmail is required"),
                FromEmail = this.JObject["fromEmail"]?.ToString() ?? throw new InvalidOperationException("FromEmail is required")
            }
        };
    }
}
