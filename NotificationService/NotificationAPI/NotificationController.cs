using Microsoft.AspNetCore.Mvc;
using NotificationAPI.Model;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody] NotificationWithEmailData notificationWithEmailData)
    {
        Console.WriteLine("NotificationController.SendNotification()");
        await _notificationService.SendNotification(notificationWithEmailData);
        return Ok("Notifications sent successfully");
    }
}

