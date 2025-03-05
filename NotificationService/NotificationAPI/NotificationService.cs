using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NotificationAPI.Model;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Mvc;

public interface INotificationService
{
    Task<IActionResult> SendNotification(NotificationWithEmailData notificationWithEmailData);
}

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly List<NotificationPreference> _preferences;

    public NotificationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _preferences = _configuration.GetSection("preferences").Get<List<NotificationPreference>>();
    }

    public async Task<IActionResult> SendNotification(NotificationWithEmailData notificationWithEmailData)
    {
        Console.WriteLine("NotificationService.SendNotification()");

        var messageRequest = new MessageRequest
        {
            Message = JsonSerializer.Serialize(notificationWithEmailData)
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(messageRequest), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("http://localhost:5204/api/messagequeue/publish", jsonContent);
            return new OkObjectResult(new { Status = "Notification sent successfully" });
        }
        catch (Exception ex)
        {
            return await Task.FromResult(new ObjectResult(new { Status = "Failed to publish message", Error = ex.Message })
            {
                StatusCode = 500
            });
        }
    }

    public class MessageRequest
    {
        public string Message { get; set; }
    }
}