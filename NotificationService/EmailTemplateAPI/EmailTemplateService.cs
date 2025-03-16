using System.Text.Json;
using System.Text;
using static EmailTemplateBackgroundService;
using static EventNameToEmailTemplateNameMapper;
using EmailTemplateAPI.Handlebars;

public interface IEmailTemplateService
{
    IEnumerable<EmailTemplate> GetAllTemplates();
    EmailTemplate? GetTemplateById(int id);
    void CreateTemplate(EmailTemplate template);
    
    Task<OutboundEmailMessage> ProcessEmailTemplateAsync(EmailActivity emailActivity, string userLanguage, CancellationToken cancellationToken);
        
    Task<bool> PublishProcessedEmailAsync(OutboundEmailMessage email, Guid notificationGuid, HttpClient client, CancellationToken cancellationToken);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IEmailTemplateRepository _repository;
    private readonly ILogger<EmailTemplateService> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public EmailTemplateService(
        IEmailTemplateRepository repository, 
        ILogger<EmailTemplateService> logger,
        IHostEnvironment hostEnvironment)
    {
        _repository = repository;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public IEnumerable<EmailTemplate> GetAllTemplates() => _repository.GetAll();

    public EmailTemplate? GetTemplateById(int id) => _repository.GetById(id);

    public void CreateTemplate(EmailTemplate template) => _repository.Add(template);

    /// <summary>
    /// Process an email activity into a ready-to-send email by applying the appropriate template
    /// </summary>
    public async Task<OutboundEmailMessage> ProcessEmailTemplateAsync(EmailActivity emailActivity, string userLanguage, CancellationToken cancellationToken)
    {
        // Get template name based on activity type
        string templateName = GetEmailTemplateNameFromEventName(emailActivity.ActivityType);
        
        // Get template paths
        string htmlTemplatePath = Path.Combine(
            _hostEnvironment.ContentRootPath, 
            "Handlebars", 
            "SystemEmailTemplates", 
            $"{templateName}.hbs");
            
        string textTemplatePath = Path.Combine(
            _hostEnvironment.ContentRootPath, 
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
        var outboundEmailMessage = new OutboundEmailMessage
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
    public async Task<bool> PublishProcessedEmailAsync(OutboundEmailMessage email, Guid notificationGuid, HttpClient client, CancellationToken cancellationToken)
    {
        try
        {
            string publishUrl = "http://localhost:5204/api/messagequeue/publish/EmailTemplateHasBeenPopulated";

            // We need to add the notificationGuid
            var payload = new
            {
                toEmail = email.ToEmail,
                fromEmail = email.FromEmail,
                subject = email.Subject,
                htmlBody = email.HtmlBody,
                textBody = email.TextBody,
                notificationGuid = notificationGuid.ToString()
            };

            var publishContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(publishUrl, publishContent, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Email queued for sending: {ResponseBody}", responseBody);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to queue email for sending: {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing processed email to queue");
            return false;
        }
    }
}