// <copyright file="JsonData.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the JSON data in the message.
/// </summary>
public class JsonData
{
    /// <summary>
    /// Gets or sets the modifier display name.
    /// </summary>
    [JsonPropertyName("modifierDisplayName")]
    required public string ModifierDisplayName { get; set; }

    /// <summary>
    /// Gets or sets the case id.
    /// </summary>
    [JsonPropertyName("caseId")]
    required public string CaseId { get; set; }

    /// <summary>
    /// Gets or sets the case title.
    /// </summary>
    [JsonPropertyName("caseTitle")]
    required public string CaseTitle { get; set; }

    /// <summary>
    /// Gets or sets the doc title.
    /// </summary>
    [JsonPropertyName("docTitle")]
    required public string DocTitle { get; set; }
}
