public static class EventNameToEmailTemplateNameMapper
{
    public const string DocumentUploadedToCase = "DocumentUploadedToCase";

    public static string GetEmailTemplateNameFromEventName(string eventName)
    {
        return eventName switch
        {
            "dk.openesdh.case.document-upload" => DocumentUploadedToCase,
            _ => throw new ArgumentException($"Unknown event name: {eventName}")
        };
    }
}