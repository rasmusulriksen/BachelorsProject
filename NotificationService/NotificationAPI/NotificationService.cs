using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Client;
using NotificationAPI.Model;
using Microsoft.Extensions.Configuration;

public interface INotificationService
{
    Task SendNotification(InboundNotification inboundNotification);
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

    public async Task SendNotification(InboundNotification inboundNotification)
    {
        var userPreference = _preferences.FirstOrDefault(p => p.UserName == inboundNotification.UserName);
        if (userPreference == null) return;

        if (userPreference.InAppEnabled)
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:8000/alfresco/wcs/api/openesdh/notifications", inboundNotification);
            response.EnsureSuccessStatusCode();
        }

        if (userPreference.EmailEnabled)
        {
            await _daprClient.PublishEventAsync("pubsub", "populate-email-template", inboundNotification);
        }
    }
}



