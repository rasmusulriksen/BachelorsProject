public interface IEmailTemplateRepository
{
    IEnumerable<EmailTemplate> GetAll();
    EmailTemplate? GetById(int id);
    void Add(EmailTemplate template);
}

public class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly List<EmailTemplate> _templates = new List<EmailTemplate>();

    public IEnumerable<EmailTemplate> GetAll() => _templates;

    public EmailTemplate? GetById(int id) => _templates.FirstOrDefault(t => t.Id == id);

    public void Add(EmailTemplate template) => _templates.Add(template);
}