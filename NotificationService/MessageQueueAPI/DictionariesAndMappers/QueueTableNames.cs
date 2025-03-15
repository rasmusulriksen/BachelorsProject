// This class contains the names of the queue tables in the database.
// The queue tables are used to store the events that occur in the system.
// This reduces the amounts of magic strings in the code.

namespace MessageQueueAPI
{
    public static class QueueTableNames
    {
            public const string Notifications = "queues.notifications";
            public const string EmailsToBePopulated = "queues.emails_to_be_merged_into_template";
            public const string EmailsToBeSent = "queues.emails_to_be_sent";
    }
} 