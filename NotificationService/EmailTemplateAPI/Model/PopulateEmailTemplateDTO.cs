public class PopulateEmailTemplateDTO
{
    public string TemplateName { get; set; }
    public List<string> DynamicEmailFields { get; set; }

    public PopulateEmailTemplateDTO(string templateName, List<string> dynamicEmailFields)
    {
        TemplateName = templateName;
        DynamicEmailFields = dynamicEmailFields;
    }
}