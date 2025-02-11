public interface IEmailTemplateService
{
    IEnumerable<EmailTemplate> GetAllTemplates();
    EmailTemplate? GetTemplateById(int id);
    void CreateTemplate(EmailTemplate template);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IEmailTemplateRepository _repository;

    public EmailTemplateService(IEmailTemplateRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<EmailTemplate> GetAllTemplates() => _repository.GetAll();

    public EmailTemplate? GetTemplateById(int id) => _repository.GetById(id);

    public void CreateTemplate(EmailTemplate template) => _repository.Add(template);
}