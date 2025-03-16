using System.Text;
using System.Text.Json;
using Model;
using NotificationAPI.Model;

public class NotificationPollingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationPollingService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    private readonly IConfiguration _configuration;
    private readonly List<NotificationPreference> _preferences;
    private readonly INotificationService _notificationService;

    public NotificationPollingService(
        IHttpClientFactory httpClientFactory, 
        ILogger<NotificationPollingService> logger, 
        IConfiguration configuration,
        INotificationService notificationService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _preferences = _configuration.GetSection("preferences").Get<List<NotificationPreference>>();
        _notificationService = notificationService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NotificationPollingService started.");

        while (!cancellationToken.IsCancellationRequested)
        {   
            try
            {
                var client = _httpClientFactory.CreateClient("MessageQueueClient");
                
                // Add this line to set the Referer header explicitly
                client.DefaultRequestHeaders.Referrer = new Uri("http://localhost:5258");
                
                var response = await client.GetAsync("http://localhost:5204/api/messagequeue/poll", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                    return;
                }

                // Read notifications as List of QueueMessage
                List<QueueMessage> notifications = await response.Content.ReadFromJsonAsync<List<QueueMessage>>(cancellationToken: cancellationToken);

                foreach (var notification in notifications)
                {
                    // Deserialize the message
                    var message = JsonSerializer.Deserialize<NotificationFromAlfresco>(notification.Message);

                    // Find user preference
                    var preference = _preferences.FirstOrDefault(p => p.Email == message.ToEmail);
                    if (preference != null)
                    {
                        _logger.LogInformation("Fetched preference for user: {UserName}", preference.Email);

                        if (preference.LinksEnabled)
                        {
                            message.LinksEnabled = true;
                        }

                        // Send email notification if enabled
                        if (preference.EmailEnabled)
                        {
                            await _notificationService.CreateEmailNotification(message, notification.NotificationGuid, cancellationToken);
                        }

                        // Send in-app notification if enabled
                        if (preference.InAppEnabled)
                        {
                            await _notificationService.CreateInAppNotification(message, cancellationToken);
                        }

                        // Mark the notification as done
                        // But when is a notification actually done? Who is responsible for updating the status?
                        // Should the processing_status have more states? I.e. "EmailSent", "InAppSent" etc?
                        // Or is the "processing_status" column in the queues.notifications table only related to the processing of the nofitication taking place in this API?
                        // In this case, it's fair to say that the notification is done when it has been processed by this API.
                        await client.GetAsync($"http://localhost:5204/api/messagequeue/done/{notification.NotificationGuid}", cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while polling for or processing notifications.");
            }

            await Task.Delay(_pollingInterval, cancellationToken);
        }

        _logger.LogInformation("Email polling service is stopping.");
    }
}