using System.Text.Json;

namespace NotificationAPI.Model;

public class NotificationWithEmailData
{
    public string NotificationType { get; set; }
    public string JsonData { get; set; }
    public string UserName { get; set; }
    public string ToEmail { get; set; }
    public string FromEmail { get; set; }

    public NotificationWithEmailData(string notificationType, string jsonData, string userName, string toEmail, string fromEmail)
    {
        NotificationType = notificationType;
        JsonData = jsonData;
        UserName = userName;
        ToEmail = toEmail;
        FromEmail = fromEmail;
    }

    public NotificationWithEmailData() { }
}
