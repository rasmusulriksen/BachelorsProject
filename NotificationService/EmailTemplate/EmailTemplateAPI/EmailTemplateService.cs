using Dapr.Client;

public interface IEmailTemplateService
{
    IEnumerable<EmailTemplate> GetAllTemplates();
    EmailTemplate? GetTemplateById(int id);
    void CreateTemplate(EmailTemplate template);

    EmailReadyToSend PopulateSystemEmail(PopulateEmailTemplateDTO dto);

    void PublishTestEmailToDaprQueue(EmailReadyToSend email);
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

    public EmailReadyToSend PopulateSystemEmail(PopulateEmailTemplateDTO dto)
    {
        var templatePath = "EmailTemplateAPI/SystemEmailTemplates/" + dto.TemplateName + ".html";

        var emailTemplate = File.ReadAllText(templatePath);
        foreach (var field in dto.DynamicEmailFields)
        {
            emailTemplate = emailTemplate.Replace("{{" + field + "}}", field);
        }

        return new EmailReadyToSend(
            "rasmus.ulriksen@visma.com",
            "test@test.dk",
            "Test Email",
            emailTemplate
        );
    }

    public void PublishTestEmailToDaprQueue(EmailReadyToSend email)
    {
        var daprClient = new DaprClientBuilder().Build();
        daprClient.PublishEventAsync("pubsub", "EmailTemplatePopulated", email);
    }
}