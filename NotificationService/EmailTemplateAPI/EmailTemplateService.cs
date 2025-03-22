// <copyright file="EmailTemplateService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailTemplateAPI;

using System.Text.Json;
using System.Text;
using static EmailTemplateBackgroundService;
using static EventNameToEmailTemplateNameMapper;
using Visma.Ims.EmailTemplateAPI.Handlebars;

/// <summary>
/// Interface for the EmailTemplateService.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Get all email templates.
    /// </summary>
    /// <returns>A list of all email templates.</returns>
    IEnumerable<EmailTemplate> GetAllTemplates();

    /// <summary>
    /// Get an email template by its ID.
    /// </summary>
    /// <param name="id">The ID of the email template to get.</param>
    /// <returns>The email template with the given ID, or null if it does not exist.</returns>
    EmailTemplate? GetTemplateById(int id);

    /// <summary>
    /// Create a new email template.
    /// </summary>
    /// <param name="template">The email template to create.</param>
    void CreateTemplate(EmailTemplate template);

    /// <summary>
    /// Process an email activity into a ready-to-send email by applying the appropriate template.
    /// </summary>
    /// <param name="emailActivity">The email activity to process.</param>
    /// <param name="userLanguage">The language of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The processed email message.</returns>
    Task<OutboundEmail> ProcessEmailTemplateAsync(EmailActivity emailActivity, string userLanguage, CancellationToken cancellationToken);

    /// <summary>
    /// Publish a processed email to the outbound email queue.
    /// </summary>
    /// <param name="email">The email to publish.</param>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="client">The HTTP client.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the email was published successfully, false otherwise.</returns>
    Task<bool> PublishProcessedEmailAsync(OutboundEmail email, long messageId, HttpClient client, CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of the EmailTemplateService.
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly IEmailTemplateRepository repository;
    private readonly ILogger<EmailTemplateService> logger;
    private readonly IHostEnvironment hostEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailTemplateService"/> class.
    /// </summary>
    /// <param name="repository">The repository for the email templates.</param>
    /// <param name="logger">The logger for the email templates.</param>
    /// <param name="hostEnvironment">The host environment for the email templates.</param>
    public EmailTemplateService(
        IEmailTemplateRepository repository,
        ILogger<EmailTemplateService> logger,
        IHostEnvironment hostEnvironment)
    {
        this.repository = repository;
        this.logger = logger;
        this.hostEnvironment = hostEnvironment;
    }

    /// <summary>
    /// Get all email templates.
    /// </summary>
    /// <returns>A list of all email templates.</returns>
    public IEnumerable<EmailTemplate> GetAllTemplates() => this.repository.GetAll();

    /// <summary>
    /// Get an email template by its ID.
    /// </summary>
    /// <param name="id">The ID of the email template to get.</param>
    /// <returns>The email template with the given ID, or null if it does not exist.</returns>
    public EmailTemplate? GetTemplateById(int id) => this.repository.GetById(id);

    /// <summary>
    /// Create a new email template.
    /// </summary>
    /// <param name="template">The email template to create.</param>
    public void CreateTemplate(EmailTemplate template) => this.repository.Add(template);

    /// <summary>
    /// Process an email activity into a ready-to-send email by applying the appropriate template
    /// </summary>
    /// <param name="emailActivity">The email activity to process.</param>
    /// <param name="userLanguage">The language of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The processed email message.</returns>
    public async Task<OutboundEmail> ProcessEmailTemplateAsync(EmailActivity emailActivity, string userLanguage, CancellationToken cancellationToken)
    {
        // Get template name based on activity type
        string templateName = GetEmailTemplateNameFromEventName(emailActivity.ActivityType);

        // Get template paths
        string htmlTemplatePath = Path.Combine(
            this.hostEnvironment.ContentRootPath,
            "Handlebars",
            "SystemEmailTemplates",
            $"{templateName}.hbs");

        string textTemplatePath = Path.Combine(
            this.hostEnvironment.ContentRootPath,
            "Handlebars",
            "SystemEmailTemplates",
            $"{templateName}.txt.hbs");

        // Prepare context for template
        Dictionary<string, object> context = new Dictionary<string, object>
        {
            { "language", userLanguage },
            { "activity", emailActivity },
            { "data", emailActivity.JsonData }
        };

        // Compile HTML template
        string htmlTemplateText = await File.ReadAllTextAsync(htmlTemplatePath, cancellationToken);
        string htmlContent = HandlebarsHelperExtensions.CompileTemplate(htmlTemplateText, context);

        // Try to compile text template
        string textContent = "";
        if (File.Exists(textTemplatePath))
        {
            string textTemplateText = await File.ReadAllTextAsync(textTemplatePath, cancellationToken);
            textContent = HandlebarsHelperExtensions.CompileTemplate(textTemplateText, context);
        }

        // Create the email message
        var outboundEmailMessage = new OutboundEmail
        {
            ToEmail = emailActivity.ToEmail,
            FromEmail = emailActivity.FromEmail,
            Subject = $"Document uploaded to case: {emailActivity.JsonData.CaseTitle}",
            HtmlBody = htmlContent,
            TextBody = textContent
        };

        return outboundEmailMessage;
    }

    /// <summary>
    /// Publish a processed email to the outbound email queue
    /// </summary>
    /// <param name="email">The email to publish.</param>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="client">The HTTP client.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the email was published successfully, false otherwise.</returns>
    public async Task<bool> PublishProcessedEmailAsync(OutboundEmail email, long messageId, HttpClient client, CancellationToken cancellationToken)
    {
        try
        {
            string publishUrl = "http://localhost:5204/api/messagequeue/publish/EmailTemplateHasBeenPopulated";

            var publishContent = new StringContent(JsonSerializer.Serialize(email), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(publishUrl, publishContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                this.logger.LogInformation("Email queued for sending: {ResponseBody}, MessageId: {MessageId}", responseBody, messageId);
                return true;
            }
            else
            {
                this.logger.LogWarning("Failed to queue email for sending: {StatusCode}, MessageId: {MessageId}", response.StatusCode, messageId);
                return false;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error publishing processed email to queue for MessageId: {MessageId}", messageId);
            return false;
        }
    }
}