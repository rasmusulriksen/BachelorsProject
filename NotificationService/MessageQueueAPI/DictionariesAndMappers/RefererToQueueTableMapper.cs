// <copyright file="RefererToQueueTableMapper.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

/// <summary>
/// This class maps referer URLs to the corresponding queue table names.
/// It is used to determine which queue table to use based on the referer URL.
/// The referer URL is the URL of the service that is calling the MessageQueueAPI.
/// The queue table name is the name of the table in the database that contains the queue.
/// </summary>
public static class RefererToQueueTableMapper
{
    private static readonly Dictionary<string, string> RefererToQueueMap = new Dictionary<string, string>
        {
            { "http://localhost:5258", QueueTableNames.UnprocessedNotifications },
            { "http://localhost:5298", QueueTableNames.EmailsToBePopulated },
            { "http://localhost:5089", QueueTableNames.EmailsToBeSent },
        };

    /// <summary>
    /// Gets the queue table name for a referer.
    /// </summary>
    /// <param name="refererUrl">The referer URL.</param>
    /// <returns>The queue table name.</returns>
    public static string GetQueueTableName(string refererUrl)
    {
        if (string.IsNullOrEmpty(refererUrl))
        {
            throw new ArgumentException("Referer URL cannot be null or empty");
        }

        // Normalizing the refererURL to avoid issues with trailing slashes and format inconsistensiers
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

        if (RefererToQueueMap.TryGetValue(origin, out string? queueTableName))
        {
            return queueTableName;
        }

        throw new ArgumentException($"No queue table mapped for referer: {origin}");
    }
}
