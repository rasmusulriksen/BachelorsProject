using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Model;
using NotificationAPI.Model;

public class NotificationPollingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationPollingService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    private readonly IConfiguration _configuration;
    private readonly List<NotificationPreference> _preferences;

    public NotificationPollingService(IHttpClientFactory httpClientFactory, ILogger<NotificationPollingService> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _preferences = _configuration.GetSection("preferences").Get<List<NotificationPreference>>();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NotificationPollingService started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MessageQueueClient");
                var response = await client.GetAsync("http://localhost:5204/api/messagequeue/poll/NotificationInitialized", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No new emails or processing failed: {StatusCode}", response.StatusCode);
                    return;
                }

                // Read notifications as List of tuples
                List<IdAndMessage> notifications = await response.Content.ReadFromJsonAsync<List<IdAndMessage>>(cancellationToken: cancellationToken);

                foreach (var notification in notifications)
                {
                    // Deserialize the message
                    var message = JsonSerializer.Deserialize<NotificationWithEmailData>(notification.Message);

                    // Find user preference
                    var preference = _preferences.FirstOrDefault(p => p.UserName == message.UserName);
                    if (preference != null)
                    {
                        _logger.LogInformation("Fetched preference for user: {UserName}", preference.UserName);

                        if (preference.EmailEnabled)
                        {
                            string url = "http://localhost:5204/api/messagequeue/publish/EmailTemplateShouldBePopulated";
                            var emailContent = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");
                            var response2 = await client.PostAsync(url, emailContent, cancellationToken);
                            var responseBody2 = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation("Email sent: {ResponseBody}", responseBody2);
                        }

                        if (preference.InAppEnabled)
                        {
                            var inAppNotification = new InAppNotification
                            {
                                ActivityType = message.ActivityType,
                                JsonData = message.JsonData,
                                UserId = message.UserName
                            };

                            var serializedInAppNotification = JsonSerializer.Serialize(inAppNotification);

                            var inAppNotificationContent = new StringContent(serializedInAppNotification, Encoding.UTF8, "application/json");

                            var url = "http://localhost:8000/alfresco/wcs/api/openesdh/notifications";
                            var response3 = await _httpClientFactory.CreateClient().PostAsync(url, inAppNotificationContent, cancellationToken);
                            var responseBody3 = await response3.Content.ReadAsStringAsync();
                            _logger.LogInformation("In-app notification sent: {ResponseBody}", responseBody3);
                        }

                        // Mark the notification as done
                        await client.GetAsync("http://localhost:5204/api/messagequeue/done/" + notification.Id, cancellationToken);
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