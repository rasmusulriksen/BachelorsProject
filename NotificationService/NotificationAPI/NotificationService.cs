using System.Text.Json;
using NotificationAPI.Model;
using System.Text;

public interface INotificationService
{
    Task<string> CreateEmailNotification(NotificationFromAlfresco message, CancellationToken cancellationToken = default);
    Task<string> CreateInAppNotification(NotificationFromAlfresco message, CancellationToken cancellationToken = default);
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public NotificationService(ILogger<NotificationService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> CreateEmailNotification(NotificationFromAlfresco message, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MessageQueueClient");
            string url = "http://localhost:5204/api/messagequeue/publish/EmailTemplateShouldBePopulated";
            
            var emailContent = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, emailContent, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogInformation("Email sent: {ResponseBody}", responseBody);
            return responseBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notification");
            throw;
        }
    }

    public async Task<string> CreateInAppNotification(NotificationFromAlfresco message, CancellationToken cancellationToken = default)
    {
        try
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
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(url, inAppNotificationContent, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogInformation("In-app notification sent: {ResponseBody}", responseBody);
            return responseBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending in-app notification");
            throw;
        }
    }
}