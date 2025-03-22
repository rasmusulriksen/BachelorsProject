// <copyright file="EmailSenderService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailSenderAPI;

using System.Net.Mail;
using Visma.Ims.Common.Abstractions.Logging;

/// <summary>
/// Represents the email sender service.
/// </summary>
public interface IEmailSenderService
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body of the email.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendEmailAsync(string subject, string body);
}

/// <summary>
/// Sends an email asynchronously.
/// </summary>
public class EmailSenderService : IEmailSenderService
{
    private readonly ILogFactory logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSenderService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public EmailSenderService(ILogFactory logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body of the email.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendEmailAsync(string subject, string body)
    {
        using (var client = new SmtpClient("localhost", 2526))
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
            this.logger.Log().Information("Email sent successfully");
        }
    }
}

/// <summary>
/// Represents the email ready to send.
/// </summary>
/// <param name="subject">The subject of the email.</param>
/// <param name="body">The body of the email.</param>
public record EmailReadyToSend(string subject, string body);
