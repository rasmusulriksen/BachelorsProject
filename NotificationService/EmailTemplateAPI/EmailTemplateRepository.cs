// <copyright file="EmailTemplateRepository.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailTemplateAPI;

/// <summary>
/// Repository for managing email templates.
/// </summary>
public interface IEmailTemplateRepository
{
    /// <summary>
    /// Get all email templates.
    /// </summary>
    /// <returns>A list of all email templates.</returns>
    IEnumerable<EmailTemplate> GetAll();

    /// <summary>
    /// Get an email template by its ID.
    /// </summary>
    /// <param name="id">The ID of the email template to get.</param>
    /// <returns>The email template with the given ID.</returns>
    EmailTemplate? GetById(int id);

    /// <summary>
    /// Add a new email template.
    /// </summary>
    /// <param name="template">The email template to add.</param>
    void Add(EmailTemplate template);
}

/// <summary>
/// Repository for managing email templates.
/// </summary>
public class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly List<EmailTemplate> templates = new List<EmailTemplate>();

    /// <inheritdoc/>
    public IEnumerable<EmailTemplate> GetAll() => this.templates;

    /// <inheritdoc/>
    public EmailTemplate? GetById(int id) => this.templates.FirstOrDefault(t => t.Id == id);

    /// <inheritdoc/>
    public void Add(EmailTemplate template) => this.templates.Add(template);
}
