using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateService _service;

    public EmailTemplatesController(IEmailTemplateService service)
    {
        _service = service;
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
}