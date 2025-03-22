// <copyright file="OutboundEmail.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

using System.Text.Json.Serialization;

/// <summary>
/// Represents an outbound email message.
/// </summary>
public class OutboundEmail
{
    /// <summary>
    /// Gets or sets the email address of the recipient.
    /// </summary>
    [JsonPropertyName("toEmail")]
    public string ToEmail { get; set; }

    /// <summary>
    /// Gets or sets the email address of the sender.
    /// </summary>
    [JsonPropertyName("fromEmail")]
    public string FromEmail { get; set; }

    /// <summary>
    /// Gets or sets the subject of the email.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the HTML body of the email.
    /// </summary>
    [JsonPropertyName("htmlBody")]
    public string HtmlBody { get; set; }

    /// <summary>
    /// Gets or sets the text body of the email.
    /// </summary>
    [JsonPropertyName("textBody")]
    public string TextBody { get; set; }
}
