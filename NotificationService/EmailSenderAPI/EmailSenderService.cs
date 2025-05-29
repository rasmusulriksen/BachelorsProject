// <copyright file="EmailSenderService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailSenderAPI;

using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Visma.Ims.EmailSenderAPI.Configuration;
using Visma.Ims.EmailSenderAPI.Model;
using Visma.Ims.Common.Infrastructure.Logging;
using Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Represents the email sender service.
/// </summary>
public interface IEmailSenderService
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="outboundEmail">The outbound email to send.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendEmailAsync(OutboundEmail outboundEmail);
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
    /// <param name="outboundEmail">The outbound email to send.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendEmailAsync(OutboundEmail outboundEmail)
    {
        using (var client = new SmtpClient("smtp4dev", 25))
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(outboundEmail.FromEmail),
                Subject = outboundEmail.Subject,
                Body = outboundEmail.HtmlBody,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(outboundEmail.ToEmail);

            await client.SendMailAsync(mailMessage);
            this.logger.Log().Information("Email sent successfully to {ToEmail} with subject '{Subject}'", outboundEmail.ToEmail, outboundEmail.Subject);
        }
    }
}

/// <summary>
/// Represents the email ready to send.
/// </summary>
/// <param name="subject">The subject of the email.</param>
/// <param name="body">The body of the email.</param>
public record EmailReadyToSend(string subject, string body);
