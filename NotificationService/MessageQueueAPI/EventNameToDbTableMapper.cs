public static class EventNameToDbTableMapper
{
    public const string NotificationInitialized = "queues.notifications";
    public const string EmailTemplateShouldBePopulated = "queues.emails_to_be_merged_into_templates";
    public const string EmailTemplateHasBeenPopulated = "queues.emails_to_be_sent";

    public static string GetDbTableForEventName(string eventName)
    {
        return eventName switch
        {
            "NotificationInitialized" => NotificationInitialized,
            "EmailTemplateShouldBePopulated" => EmailTemplateShouldBePopulated,
            "EmailTemplateHasBeenPopulated" => EmailTemplateHasBeenPopulated,
            _ => throw new ArgumentException($"Unknown event name: {eventName}")
        };
    }
}