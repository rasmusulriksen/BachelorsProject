// This class maps referer URLs to the corresponding queue table names.
// It is used to determine which queue table to use based on the referer URL.
// The referer URL is the URL of the service that is calling the MessageQueueAPI.
// The queue table name is the name of the table in the database that contains the queue.

namespace MessageQueueAPI
{
    public static class RefererToQueueTableMapper
    {
        private static readonly Dictionary<string, string> _refererToQueueMap = new Dictionary<string, string>
        {
            { "http://localhost:5258", QueueTableNames.Notifications },
            { "http://localhost:5298", QueueTableNames.EmailsToBePopulated },
            { "http://localhost:5299", QueueTableNames.EmailsToBeSent },
        };

        // Get queue table name for a referer
        public static string GetQueueTableName(string refererUrl)
        {
            if (string.IsNullOrEmpty(refererUrl))
            {
                throw new ArgumentException("Referer URL cannot be null or empty");
            }

            // Extract the origin (protocol + host + port) from the referer URL
            Uri uri;
            try
            {
                uri = new Uri(refererUrl);
            }
            catch (UriFormatException)
            {
                throw new ArgumentException($"Invalid referer URL format: {refererUrl}");
            }

            string origin = $"{uri.Scheme}://{uri.Authority}";

            // Try to get the queue table name for the origin
            if (_refererToQueueMap.TryGetValue(origin, out string queueTableName))
            {
                return queueTableName;
            }

            throw new ArgumentException($"No queue table mapped for referer: {origin}");
        }
    }
} 