using System.Net.Mail;
using Microsoft.Extensions.Logging;

public interface IEmailSender
{
    Task SendEmailAsync(string subject, string body);
}

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
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