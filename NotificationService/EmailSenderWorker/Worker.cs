using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using Dapr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EmailSenderWorker;

public class Worker
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    [Topic("pubsub", "EmailTemplatePopulated")]
    public async Task HandleEmailTemplate(string emailContent)
    {
        if (emailContent != null)
        {
            await SendEmailAsync("Test Email", emailContent);
        }
    }

    private async Task SendEmailAsync(string subject, string body)
    {
        using (var client = new SmtpClient("localhost", 25))
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
        }
    }
}