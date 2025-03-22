// <copyright file="EmailSenderController.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailSenderAPI;

using Microsoft.AspNetCore.Mvc;
using Visma.Ims.Common.Abstractions.Logging;

/// <summary>
/// Represents the email sender controller.
/// </summary>
[ApiController]
[Route("[controller]")]
public class EmailSenderController : ControllerBase
{
    private readonly ILogFactory logger;
    private readonly IEmailSenderService emailSenderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSenderController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="emailSenderService">The email sender service.</param>
    public EmailSenderController(ILogFactory logger, IEmailSenderService emailSenderService)
    {
        this.logger = logger;
        this.emailSenderService = emailSenderService;
    }

    /// <summary>
    /// Handles the email template.
    /// </summary>
    /// <param name="email">The email ready to send.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [HttpPost("template")]
    public async Task<IActionResult> HandleEmailTemplate([FromBody] EmailReadyToSend email)
    {
        try
        {
            this.logger.Log().Information("Received email template: {@Email}", email);
            await this.emailSenderService.SendEmailAsync(email.subject, email.body);
            return this.Ok();
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error processing email template");
            return this.StatusCode(500);
        }
    }
}
