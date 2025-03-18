// <copyright file="EmailNotification.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Email notification model.
/// </summary>
public class EmailNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailNotification"/> class.
    /// </summary>
    /// <param name="activityType">The activity type.</param>
    /// <param name="jsonData">The JSON data.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="toEmail">The to email.</param>
    /// <param name="fromEmail">The from email.</param>
    /// <param name="linksEnabled">A value indicating whether links are enabled.</param>
    public EmailNotification(string activityType, JsonData jsonData, string userName, string toEmail, string fromEmail, bool linksEnabled)
    {
        this.ActivityType = activityType;
        this.JsonData = jsonData;
        this.UserName = userName;
        this.ToEmail = toEmail;
        this.FromEmail = fromEmail;
        this.LinksEnabled = linksEnabled;
    }

    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    public string ActivityType { get; set; }

    /// <summary>
    /// Gets or sets the JSON data.
    /// </summary>
    public JsonData JsonData { get; set; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the to email.
    /// </summary>
    public string ToEmail { get; set; }

    /// <summary>
    /// Gets or sets the from email.
    /// </summary>
    public string FromEmail { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether links are enabled.
    /// </summary>
    public bool LinksEnabled { get; set; } = false; // Default to false
}
