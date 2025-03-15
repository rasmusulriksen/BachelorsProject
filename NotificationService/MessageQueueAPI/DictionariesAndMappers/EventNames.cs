// This class contains the event names that are used to identify the type of event that occurred.
// The event names are used to determine which queue table to use based on the event name.

namespace MessageQueueAPI
{
    public static class EventNames
    {
        public const string NotificationInitialized = "NotificationInitialized";
        public const string EmailTemplateShouldBePopulated = "EmailTemplateShouldBePopulated";
        public const string EmailTemplateHasBeenPopulated = "EmailTemplateHasBeenPopulated";
    }
}