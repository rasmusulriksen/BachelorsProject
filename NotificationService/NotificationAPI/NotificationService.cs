using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NotificationAPI.Model;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Model;

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

        var preference = _preferences.FirstOrDefault(p => p.UserName == "rasmus.ulriksen");

        if (preference?.InAppEnabled ?? false)
        {
            Console.WriteLine("[NotificationService.SendNotification(): Sending in-app notification]");

            var inAppNotification = new InAppNotification
            {
                ActivityType = "dk.openesdh.case.document-upload",
                JsonData = new JsonData
                {
                    DocRecordNodeRef = "workspace://SpacesStore/367a583f-77af-4551-b2d1-0ae59305e9b3",
                    ModifierDisplayName = "NotificationAPI",
                    Modifier = "admin",
                    CaseId = "20250305-3159",
                    CaseTitle = "Muligt salg til en rig islænding",
                    DocTitle = "hemmeligt_bud.pdf",
                    ParentTitle = "Muligt salg til en rig islænding",
                    ParentType = "simple:case",
                    ParentRef = "workspace://SpacesStore/03bbc808-2172-406f-8800-f612f2771ea2"
                },
                UserId = "rasmus.ulriksen"
            };

            var serializedInAppNotification = JsonSerializer.Serialize(inAppNotification);
            Console.WriteLine("Serialized Notification Payload: " + serializedInAppNotification);

            var inAppContent = new StringContent(serializedInAppNotification, Encoding.UTF8, "application/json");

            try
            {
                var url = "http://localhost:8000/alfresco/wcs/api/openesdh/notifications";
                Console.WriteLine($"Sending POST request to {url}");

                var response = await _httpClient.PostAsync(url, inAppContent);
                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body: " + responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error occurred while sending notification.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: " + ex.Message);
            }
        }

        if (preference?.EmailEnabled ?? false)
        {
            // send notificationWithEmailData to messagequeueapi (localhost:5204/api/messagequeue/publish)
            var serializedNotificationWithEmailData = JsonSerializer.Serialize(notificationWithEmailData);
            Console.WriteLine("Serialized NotificationWithEmailData Payload: " + serializedNotificationWithEmailData);

            var emailContent = new StringContent(serializedNotificationWithEmailData, Encoding.UTF8, "application/json");

            try
            {
                var url = "http://localhost:5204/api/messagequeue/publish/EmailTemplateShouldBePopulated";
                Console.WriteLine($"Sending POST request to {url}");

                var response = await _httpClient.PostAsync(url, emailContent);
                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body: " + responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error occurred while sending notification.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: " + ex.Message);
            }
        }

        return new OkObjectResult(new { Status = "Notification sent successfully" });
    }
}