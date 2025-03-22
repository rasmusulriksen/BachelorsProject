// <copyright file="EmailTemplateController.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailTemplateAPI;

using Microsoft.AspNetCore.Mvc;
using Visma.Ims.Common.Abstractions.Logging;

/// <summary>
/// Controller for managing email templates.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateService service;
    private readonly ILogFactory logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailTemplatesController"/> class.
    /// </summary>
    /// <param name="service">The email template service.</param>
    /// <param name="logger">The logger.</param>
    public EmailTemplatesController(IEmailTemplateService service, ILogFactory logger)
    {
        this.service = service;
        this.logger = logger;
    }

    /// <summary>
    /// Get all email templates.
    /// </summary>
    /// <returns>A list of all email templates.</returns>
    [HttpGet]
    public ActionResult<IEnumerable<EmailTemplate>> GetAll()
    {
        return this.Ok(this.service.GetAllTemplates());
    }

    /// <summary>
    /// Get an email template by its ID.
    /// </summary>
    /// <param name="id">The ID of the email template to get.</param>
    /// <returns>The email template with the given ID.</returns>
    [HttpGet("{id}")]
    public ActionResult<EmailTemplate> GetById(int id)
    {
        var template = this.service.GetTemplateById(id);
        if (template == null)
        {
            this.logger.Log().Warning("Email template not found: {Id}", id);
            return this.NotFound();
        }

        return this.Ok(template);
    }

    /// <summary>
    /// Create a new email template.
    /// </summary>
    /// <param name="template">The email template to create.</param>
    /// <returns>The created email template.</returns>
    [HttpPost]
    public IActionResult Create(EmailTemplate template)
    {
        this.service.CreateTemplate(template);
        return this.CreatedAtAction(nameof(this.GetById), new { id = template.Id }, template);
    }
}
