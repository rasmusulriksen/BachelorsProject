// This class maps event names to the corresponding database table names.
// It is used to determine which queue table to use based on the event name.
// This reduces the amounts of magic strings in the code.

namespace MessageQueueAPI
{
    public static class EventNameToDbTableMapper
    {
        public static string GetDbTableForEventName(string eventName)
        {
            return eventName switch
            {
                EventNames.NotificationInitialized => QueueTableNames.Notifications,
                EventNames.EmailTemplateShouldBePopulated => QueueTableNames.EmailsToBePopulated,
                EventNames.EmailTemplateHasBeenPopulated => QueueTableNames.EmailsToBeSent,
                _ => throw new ArgumentException($"Unknown event name: {eventName}")
            };
        }
    }
}