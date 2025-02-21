using System.Net.Mail;
using Microsoft.Extensions.Logging;

public interface IEmailSenderService
{
    Task SendEmailAsync(string subject, string body);
}

public class EmailSenderService : IEmailSenderService
{
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(ILogger<EmailSenderService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string subject, string body)
    {
        using (var client = new SmtpClient("smtp4dev", 25))
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("noreply@example.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add("recipient@example.com");

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully");
        }
    }
}

public record EmailReadyToSend(string Subject, string Body);