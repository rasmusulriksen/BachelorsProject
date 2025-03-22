// <copyright file="IdAndJObject.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using Visma.Ims.EmailSenderAPI.Model;

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
    /// Converts the IdAndJObject to an IdAndOutboundEmail.
    /// </summary>
    /// <returns>An IdAndOutboundEmail.</returns>
    public IdAndOutboundEmail ToIdAndOutboundEmail()
    {
        return new IdAndOutboundEmail
        {
            Id = this.Id,
            OutboundEmail = new OutboundEmail
        {
            ToEmail = this.GetPropertyCaseInsensitive("toEmail") ?? throw new InvalidOperationException("ToEmail is required"),
            FromEmail = this.GetPropertyCaseInsensitive("fromEmail") ?? throw new InvalidOperationException("FromEmail is required"),
            Subject = this.GetPropertyCaseInsensitive("subject") ?? throw new InvalidOperationException("Subject is required"),
            HtmlBody = this.GetPropertyCaseInsensitive("htmlBody") ?? throw new InvalidOperationException("HtmlBody is required"),
            TextBody = this.GetPropertyCaseInsensitive("textBody") ?? throw new InvalidOperationException("TextBody is required")
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
