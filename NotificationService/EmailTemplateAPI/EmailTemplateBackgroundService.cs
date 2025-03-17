using System.Text.Json;
using System.Text.Json.Serialization;
using EmailTemplateAPI.Handlebars;

public class EmailTemplateBackgroundService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmailTemplateBackgroundService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);
    private readonly IEmailTemplateService _emailTemplateService;

    public EmailTemplateBackgroundService(
        IHttpClientFactory httpClientFactory, 
        ILogger<EmailTemplateBackgroundService> logger, 
        IHostEnvironment hostEnvironment,
        IEmailTemplateService emailTemplateService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _emailTemplateService = emailTemplateService;
        
        // Register Handlebars helpers and partials
        HandlebarsHelperExtensions.RegisterAllHelpersAndPartials(hostEnvironment, logger);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailTemplatePollingService started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MessageQueueClient");
                
                List<QueueMessage> messages = await PollForNewMessagesAsync(client, cancellationToken);
                
                if (!messages.Any())
                {
                    await Task.Delay(_pollingInterval, cancellationToken);
                    continue;
                }

                foreach (QueueMessage message in messages)
                {
                    try 
                    {
                        await ProcessMessageAsync(message, client, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message {MessageId}", message.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in email template polling service.");
            }

            await Task.Delay(_pollingInterval, cancellationToken);
        }

        _logger.LogInformation("Email template polling service is stopping.");
    }

    private async Task<List<QueueMessage>?> PollForNewMessagesAsync(HttpClient client, CancellationToken cancellationToken)
    {
        try
        {
            client.DefaultRequestHeaders.Referrer = new Uri("http://localhost:5298");
            
            var response = await client.GetAsync("http://localhost:5204/api/messagequeue/poll", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<List<QueueMessage>>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polling for new messages");
            return null;
        }
    }

    private async Task ProcessMessageAsync(QueueMessage message, HttpClient client, CancellationToken cancellationToken)
    {
        // Default language - could be extracted from message or user preferences
        var userLanguage = "en";  

        try
        {
            EmailActivity emailActivity = JsonSerializer.Deserialize<EmailActivity>(message.Message);   

            OutboundEmailMessage outboundEmailMessage = await _emailTemplateService.ProcessEmailTemplateAsync(emailActivity, userLanguage, cancellationToken);

            bool publishSuccess = await _emailTemplateService.PublishProcessedEmailAsync(outboundEmailMessage, client, cancellationToken);

            if (publishSuccess)
            {
                await MarkMessageAsDoneAsync(client, message.Id, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}", message.Id);
            throw;
        }
    }

    private async Task MarkMessageAsDoneAsync(HttpClient client, long messageId, CancellationToken cancellationToken)
    {
        try
        {
            client.DefaultRequestHeaders.Referrer = new Uri("http://localhost:5298");   
            
            await client.GetAsync(
                $"http://localhost:5204/api/messagequeue/done/{messageId}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as done", messageId);
            throw;
        }
    }

    public class QueueMessage
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

        [JsonPropertyName("linksEnabled")]
        public bool LinksEnabled { get; set; }
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