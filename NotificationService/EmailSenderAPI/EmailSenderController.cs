using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class EmailSenderController : ControllerBase
{
    private readonly ILogger<EmailSenderController> _logger;
    private readonly IEmailSenderService _emailSenderService;

    public EmailSenderController(ILogger<EmailSenderController> logger, IEmailSenderService emailSenderService)
    {
        _logger = logger;
        _emailSenderService = emailSenderService;
    }

    [HttpPost("template")]
    public async Task<IActionResult> HandleEmailTemplate([FromBody] EmailReadyToSend email)
    {
        try
        {
            _logger.LogInformation("Received email template: {@Email}", email);
            await _emailSenderService.SendEmailAsync(email.Subject, email.Body);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email template");
            return StatusCode(500);
        }
    }
}