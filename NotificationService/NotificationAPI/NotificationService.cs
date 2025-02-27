using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Client;
using NotificationAPI.Model;
using Microsoft.Extensions.Configuration;
using System.Text;

public interface INotificationService
{
    Task SendNotification(NotificationWithEmailData notificationWithEmailData);
}

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly DaprClient _daprClient;
    private readonly IConfiguration _configuration;
    private readonly List<NotificationPreference> _preferences;

    public NotificationService(HttpClient httpClient, DaprClient daprClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _daprClient = daprClient;
        _configuration = configuration;
        _preferences = _configuration.GetSection("preferences").Get<List<NotificationPreference>>();
    }

    public async Task SendNotification(NotificationWithEmailData notificationWithEmailData)
    {

        var userPreference = _preferences.FirstOrDefault(p => p.UserName == notificationWithEmailData.UserName);
        if (userPreference == null) return;

        NotificationWithoutEmailData notificationWithoutEmailData = new NotificationWithoutEmailData(
            notificationWithEmailData.NotificationType,
            notificationWithEmailData.JsonData,
            notificationWithEmailData.UserName);

        var jsonContent = new StringContent(JsonSerializer.Serialize("hej"), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("http://localhost:8000/alfresco/wcs/api/openesdh/notifications", jsonContent);

        if (userPreference.EmailEnabled)
        {
            await _daprClient.PublishEventAsync("pubsub", "populate-email-template", notificationWithEmailData);
        }
    }
}



