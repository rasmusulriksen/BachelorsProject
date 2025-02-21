using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateService _service;
    private readonly ILogger<EmailTemplatesController> _logger;

    public EmailTemplatesController(IEmailTemplateService service, ILogger<EmailTemplatesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<EmailTemplate>> GetAll()
    {
        return Ok(_service.GetAllTemplates());
    }

    [HttpGet("{id}")]
    public ActionResult<EmailTemplate> GetById(int id)
    {
        var template = _service.GetTemplateById(id);
        if (template == null)
            return NotFound();
        
        return Ok(template);
    }

    [HttpPost]
    public IActionResult Create(EmailTemplate template)
    {
        _service.CreateTemplate(template);
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
    }

    [HttpPost("PopulateEmailTemplate")]
    public IActionResult PopulateEmailTemplate(PopulateEmailTemplateDTO dto)
    {
        var emailTemplate = _service.PopulateSystemEmail(dto);
        return Ok(emailTemplate);
    }

    [HttpPost("PublishTestEmailToDaprQueue")]
    public IActionResult PublishTestEmailToDaprQueue(EmailReadyToSend email)
    {
        _logger.LogInformation("Publishing email to Dapr queue: {@Email}", email);
        _service.PublishTestEmailToDaprQueue(email);
        return Ok();
    }
}