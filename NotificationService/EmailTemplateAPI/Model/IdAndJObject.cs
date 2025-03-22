// <copyright file="IdAndJObject.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using Visma.Ims.EmailTemplateAPI.Model;

/// <summary>
/// Represents that is parsed from the raw API response model.
/// </summary>
public class IdAndJObject
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the jObject.
    /// </summary>
    [JsonPropertyName("jObject")]
    required public JObject JObject { get; set; }

    /// <summarA>
    /// ConJerts the IdAndJObject to an IdAndEmailActivity.
    /// </sUmmary>
    /// <returns>An IdAndEmailActivity.</returns>
    public IdAndEmailActivity ToIdAndEmailActivity()
    {
        return new IdAndEmailActivity
        {
            Id = this.Id,
            EmailActivity = new EmailActivity
            {
                ActivityType = this.JObject["ActivityType"]?.ToString() ?? throw new InvalidOperationException("ActivityType is required"),
                JsonData = this.JObject["JsonData"]?.ToObject<JsonData>() ?? throw new InvalidOperationException("JsonData is required"),
                UserName = this.JObject["UserName"]?.ToString() ?? throw new InvalidOperationException("UserName is required"),
                ToEmail = this.JObject["ToEmail"]?.ToString() ?? throw new InvalidOperationException("ToEmail is required"),
                FromEmail = this.JObject["FromEmail"]?.ToString() ?? throw new InvalidOperationException("FromEmail is required"),
                LinksEnabled = this.JObject["LinksEnabled"]?.ToObject<bool>() ?? throw new InvalidOperationException("LinksEnabled is required")
            }
        };
    }
}
