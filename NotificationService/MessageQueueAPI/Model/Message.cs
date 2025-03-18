// <copyright file="Message.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI.Model;

/// <summary>
/// Represents a message in the message queue.
/// </summary>
public class Message
{
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
}

/// <summary>
/// Represents the JSON data in the message.
/// </summary>
public class JsonData
{
    /// <summary>
    /// Gets or sets the modifier display name.
    /// </summary>
    public string ModifierDisplayName { get; set; }

    /// <summary>
    /// Gets or sets the case id.
    /// </summary>
    public string CaseId { get; set; }

    /// <summary>
    /// Gets or sets the case title.
    /// </summary>
    public string CaseTitle { get; set; }

    /// <summary>
    /// Gets or sets the doc title.
    /// </summary>
    public string DocTitle { get; set; }
}
