using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Dapr.Client;

namespace EmailService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                SendTestEmail();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

            private void SendTestEmail()
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var senderName = _config["EmailSettings:SenderDisplayName"];
            var recipientEmail = _config["EmailSettings:RecipientEmail"];

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(senderEmail, senderName);
                message.To.Add(new MailAddress(recipientEmail));
                message.Subject = "Test Email";
                message.Body = "This is a test email sent from EmailService using smtp4dev.";

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = false; // SMTP4DEV doesn't require SSL
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    client.Send(message);
                }
            }

            _logger.LogInformation("Test email sent successfully at: {time}", DateTimeOffset.Now);
        }
}
