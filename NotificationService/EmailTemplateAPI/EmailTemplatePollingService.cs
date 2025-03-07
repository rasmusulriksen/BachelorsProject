using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using HandlebarsDotNet;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using EmailTemplateAPI.Handlebars;

public class EmailTemplatePollingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmailTemplatePollingService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);
    private readonly IHostEnvironment _hostEnvironment;

    public EmailTemplatePollingService(IHttpClientFactory httpClientFactory, ILogger<EmailTemplatePollingService> logger, IHostEnvironment hostEnvironment)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        
        // Register Handlebars helpers and partials
        HandlebarsHelperExtensions.RegisterAllHelpersAndPartials(_hostEnvironment, _logger);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailTemplatePollingService started.");
        
        // Log registered partials for debugging
        HandlebarsHelperExtensions.LogRegisteredPartials(_logger);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MessageQueueClient");
                var response = await client.GetAsync("http://localhost:5204/api/messagequeue/poll/EmailTemplateShouldBePopulated", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                    continue;
                }

                var messages = await response.Content.ReadFromJsonAsync<List<IdAndMessage>>(cancellationToken: cancellationToken);

                foreach (var message in messages)
                {
                    var userLanguage = "en";  // hardcoded for now

                    EmailActivity emailActivity = JsonSerializer.Deserialize<EmailActivity>(message.Message);

                    // Get template name
                    string templateName = EventNameToEmailTemplateNameMapper.GetEmailTemplateNameFromEventName(emailActivity.ActivityType);
                    
                    // Define paths for HTML and text templates
                    var htmlTemplatePath = Path.Combine(
                        _hostEnvironment.ContentRootPath, 
                        "Handlebars", 
                        "SystemEmailTemplates", 
                        $"{templateName}.hbs");
                        
                    var textTemplatePath = Path.Combine(
                        _hostEnvironment.ContentRootPath, 
                        "Handlebars", 
                        "SystemEmailTemplates", 
                        $"{templateName}.txt.hbs");

                    var context = new Dictionary<string, object>
                    {
                        { "language", userLanguage },
                        { "activity", emailActivity },
                        { "data", emailActivity.JsonData }
                    };

                    // Compile HTML template
                    string htmlTemplateText = await File.ReadAllTextAsync(htmlTemplatePath, cancellationToken);
                    string htmlContent = HandlebarsHelperExtensions.CompileTemplate(htmlTemplateText, context);
                    
                    // Try to compile text template, or use a simple fallback
                    string textContent;
                    if (File.Exists(textTemplatePath))
                    {
                        string textTemplateText = await File.ReadAllTextAsync(textTemplatePath, cancellationToken);
                        textContent = HandlebarsHelperExtensions.CompileTemplate(textTemplateText, context);
                    }
                    else
                    {
                        _logger.LogWarning("Text template not found: {TextTemplatePath}. Using HTML content as fallback.", textTemplatePath);
                        textContent = htmlContent;
                    }

                    _logger.LogInformation("Generated email HTML content: {EmailContent}", htmlContent);
                    _logger.LogInformation("Generated email text content: {EmailContent}", textContent);
                    
                    // Create a message for the outbound emails queue
                    var outboundEmailMessage = new OutboundEmailMessage
                    {
                        ToEmail = emailActivity.ToEmail,
                        FromEmail = emailActivity.FromEmail,
                        Subject = $"Document uploaded to case: {emailActivity.JsonData.CaseTitle}",
                        HtmlBody = htmlContent,
                        TextBody = textContent
                    };
                    
                    // Publish to the outbound emails queue
                    string publishUrl = "http://localhost:5204/api/messagequeue/publish/EmailTemplateHasBeenPopulated";
                    var publishContent = new StringContent(
                        JsonSerializer.Serialize(outboundEmailMessage), 
                        Encoding.UTF8, 
                        "application/json");
                    
                    var publishResponse = await client.PostAsync(publishUrl, publishContent, cancellationToken);
                    
                    if (publishResponse.IsSuccessStatusCode)
                    {
                        var responseBody = await publishResponse.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogInformation("Email queued for sending: {ResponseBody}", responseBody);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to queue email for sending: {StatusCode}", publishResponse.StatusCode);
                    }

                    // Mark as done
                    await client.GetAsync("http://localhost:5204/api/messagequeue/done/" + message.Id, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while polling for or processing emails.");
            }

            await Task.Delay(_pollingInterval, cancellationToken);
        }

        _logger.LogInformation("Email template polling service is stopping.");
    }

    public class IdAndMessage
    {
        public long Id { get; set; }
        public string Message { get; set; }
    }

    public class EmailActivity
    {
        [JsonPropertyName("activityType")]
        public string ActivityType { get; set; }

        [JsonPropertyName("jsonData")]
        public JsonData JsonData { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("toEmail")]
        public string ToEmail { get; set; }

        [JsonPropertyName("fromEmail")]
        public string FromEmail { get; set; }
    }

    public class JsonData
    {
        [JsonPropertyName("docRecordNodeRef")]
        public string DocRecordNodeRef { get; set; }
        
        [JsonPropertyName("modifierDisplayName")]
        public string ModifierDisplayName { get; set; }
        
        [JsonPropertyName("modifier")]
        public string Modifier { get; set; }
        
        [JsonPropertyName("caseId")]
        public string CaseId { get; set; }
        
        [JsonPropertyName("caseTitle")]
        public string CaseTitle { get; set; }
        
        [JsonPropertyName("docTitle")]
        public string DocTitle { get; set; }
        
        [JsonPropertyName("parentTitle")]
        public string ParentTitle { get; set; }
        
        [JsonPropertyName("parentType")]
        public string ParentType { get; set; }
        
        [JsonPropertyName("parentRef")]
        public string ParentRef { get; set; }
    }
    
    public class OutboundEmailMessage
    {
        [JsonPropertyName("toEmail")]
        public string ToEmail { get; set; }
        
        [JsonPropertyName("fromEmail")]
        public string FromEmail { get; set; }
        
        [JsonPropertyName("subject")]
        public string Subject { get; set; }
        
        [JsonPropertyName("htmlBody")]
        public string HtmlBody { get; set; }
        
        [JsonPropertyName("textBody")]
        public string TextBody { get; set; }
    }
}