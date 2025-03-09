using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class EmailSenderPollingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmailSenderPollingService> _logger;
    private readonly IEmailSenderService _emailSenderService;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    public EmailSenderPollingService(
        IHttpClientFactory httpClientFactory, 
        ILogger<EmailSenderPollingService> logger,
        IEmailSenderService emailSenderService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _emailSenderService = emailSenderService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailSenderPollingService started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MessageQueueClient");
                var response = await client.GetAsync("http://localhost:5204/api/messagequeue/poll/EmailTemplateHasBeenPopulated", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                }
                else
                {
                    var messages = await response.Content.ReadFromJsonAsync<List<IdAndMessage>>(cancellationToken: cancellationToken);

                    foreach (var message in messages)
                    {
                        try
                        {
                            var emailToSend = JsonSerializer.Deserialize<OutboundEmailMessage>(message.Message);
                            
                            _logger.LogInformation("Processing email: Subject={Subject}, To={ToEmail}", 
                                emailToSend.Subject, emailToSend.ToEmail);
                            
                            // Send the email using the existing service
                            await _emailSenderService.SendEmailAsync(emailToSend.Subject, emailToSend.HtmlBody);
                            
                            _logger.LogInformation("Email sent successfully");
                            
                            // Mark as done
                            await client.GetAsync($"http://localhost:5204/api/messagequeue/done/{message.Id}", cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing email message {MessageId}", message.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while polling for or processing emails.");
            }

            await Task.Delay(_pollingInterval, cancellationToken);
        }

        _logger.LogInformation("Email sender polling service is stopping.");
    }
}

// Models needed for the service
public record IdAndMessage(long Id, string Message);
public record OutboundEmailMessage
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