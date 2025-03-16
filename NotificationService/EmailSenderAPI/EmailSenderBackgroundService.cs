using System.Text.Json;
using System.Text.Json.Serialization;

public class EmailSenderBackgroundService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmailSenderBackgroundService> _logger;
    private readonly IEmailSenderService _emailSenderService;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    public EmailSenderBackgroundService(
        IHttpClientFactory httpClientFactory,
        ILogger<EmailSenderBackgroundService> logger,
        IEmailSenderService emailSenderService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _emailSenderService = emailSenderService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailSenderBackgroundService started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MessageQueueClient");

                client.DefaultRequestHeaders.Referrer = new Uri("http://localhost:5089");

                var response = await client.GetAsync("http://localhost:5204/api/messagequeue/poll", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                }
                else
                {
                    var messages = await response.Content.ReadFromJsonAsync<List<QueueMessage>>(cancellationToken: cancellationToken);

                    foreach (var message in messages)
                    {
                        try
                        {
                            var emailToSend = JsonSerializer.Deserialize<OutboundEmailMessage>(message.Message);

                            _logger.LogInformation("Processing email: Subject={Subject}, To={ToEmail}",
                                emailToSend.Subject, emailToSend.ToEmail);

                            await _emailSenderService.SendEmailAsync(emailToSend.Subject, emailToSend.HtmlBody);

                            _logger.LogInformation("Email sent successfully");

                            // Mark as done
                            await client.GetAsync($"http://localhost:5204/api/messagequeue/done/{message.NotificationGuid}", cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing email message {MessageId}", message.NotificationGuid);
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
public class QueueMessage
{
    public string Message { get; set; }
    public Guid NotificationGuid { get; set; }

}
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