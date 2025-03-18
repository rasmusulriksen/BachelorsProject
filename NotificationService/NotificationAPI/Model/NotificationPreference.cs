// <copyright file="NotificationPreference.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Represents a notification preference.
/// </summary>
public class NotificationPreference
{
    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    required public string Email { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether email is enabled.
    /// </summary>
    required public bool EmailEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether in-app notifications are enabled.
    /// </summary>
    public bool InAppEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether links are enabled.
    /// </summary>
    public bool LinksEnabled { get; set; }
}
