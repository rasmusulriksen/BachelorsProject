using Microsoft.AspNetCore.Mvc;
using Dapr;
using Dapr.Client;

[ApiController]
[Route("[controller]")]
public class EmailSenderController : ControllerBase
{
    private readonly ILogger<EmailSenderController> _logger;
    private readonly IEmailSender _emailSender;

    public EmailSenderController(ILogger<EmailSenderController> logger, IEmailSender emailSender)
    {
        _logger = logger;
        _emailSender = emailSender;
    }

    [Topic("pubsub", "EmailTemplatePopulated")]
    [HttpPost("template")]
    public async Task<IActionResult> HandleEmailTemplate([FromBody] EmailReadyToSend email)
    {
        try
        {
            _logger.LogInformation("Received email template: {@Email}", email);
            await _emailSender.SendEmailAsync(email.Subject, email.Body);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email template");
            return StatusCode(500);
        }
    }
}