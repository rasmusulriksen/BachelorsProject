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

    /// <summary>
    /// Converts the IdAndJObject to an IdAndEmailActivity.
    /// </summary>
    /// <returns>An IdAndEmailActivity.</returns>
    public IdAndEmailActivity ToIdAndEmailActivity()
    {
        // Log or inspect the JObject to see what it contains
        var jObjectString = this.JObject?.ToString() ?? "null";
        
        // Access JsonData using the case-insensitive method
        var jsonDataStr = this.GetPropertyCaseInsensitive("jsonData");
        if (jsonDataStr == null)
        {
            throw new InvalidOperationException($"JsonData is missing from JObject: {jObjectString}");
        }
        
        JsonData jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonData>(jsonDataStr);

        return new IdAndEmailActivity
        {
            Id = this.Id,
            EmailActivity = new EmailActivity
            {
                ActivityType = this.GetPropertyCaseInsensitive("activityType") ?? throw new InvalidOperationException("ActivityType is required"),
                JsonData = jsonData,
                UserName = this.GetPropertyCaseInsensitive("userName") ?? throw new InvalidOperationException("UserName is required"),
                ToEmail = this.GetPropertyCaseInsensitive("toEmail") ?? throw new InvalidOperationException("ToEmail is required"),
                FromEmail = this.GetPropertyCaseInsensitive("fromEmail") ?? throw new InvalidOperationException("FromEmail is required"),
                LinksEnabled = bool.Parse(this.GetPropertyCaseInsensitive("linksEnabled") ?? throw new InvalidOperationException("LinksEnabled is required"))
            }
        };
    }

    /// <summary>
    /// Gets a property value from JObject in a case-insensitive manner.
    /// </summary>
    /// <param name="propertyName">The property name to search for (case insensitive).</param>
    /// <returns>The property value as string, or null if not found.</returns>
    private string? GetPropertyCaseInsensitive(string propertyName)
    {
        // Find the property with case-insensitive comparison
        var property = this.JObject.Properties()
            .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

        return property?.Value?.ToString();
    }
}
