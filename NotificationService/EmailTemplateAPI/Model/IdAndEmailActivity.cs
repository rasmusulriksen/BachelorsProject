// <copyright file="IdAndEmailActivity.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailTemplateAPI.Model;

/// <summary>
/// Represents a strongly typed business logic model that is parsed from the raw API response model.
/// </summary>
public class IdAndEmailActivity
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    required public long Id { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    required public EmailActivity EmailActivity { get; set; }
}
