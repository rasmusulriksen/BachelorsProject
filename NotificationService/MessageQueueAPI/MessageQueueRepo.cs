using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Model;
using System;
using MessageQueueAPI;

public interface IMessageQueueRepo
{
    Task<long> EnqueueMessage(string jsonString, string eventName, Guid? notificationGuid = null);
    Task<List<IdAndMessageAndNotificationGuid>> DequeueMessages(string callingProcessorId, int numElements, string queueTableName);
    Task MarkMessageAsDone(Guid notification_guid, string processingResultText, string callingProcessorId, string queueName);
}

public class MessageQueueRepo : IMessageQueueRepo
{
    private readonly string _connectionString;

    public MessageQueueRepo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> EnqueueMessage(string jsonString, string eventName, Guid? notificationGuid = null)
    {
        Console.WriteLine("MessageQueueRepo.EnqueueMessage()");

        // If no notificationGuid is provided, this is a new notification and we thus need a new Guid for it
        if (!notificationGuid.HasValue)
        {
            notificationGuid = Guid.NewGuid();
        }

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string queueName = EventNameToDbTableMapper.GetDbTableForEventName(eventName);

            // Use a generic insert function that takes the queue name as a parameter
            string sqlCommand = "SELECT queues.insert_into_queue(@queueName, @message, @notificationGuid)";

            using (var command = new NpgsqlCommand(sqlCommand, connection))
            {
                // Add the queue name parameter
                command.Parameters.AddWithValue("queueName", queueName);
                
                // Add the message parameter as a JSON type
                command.Parameters.AddWithValue("message", NpgsqlTypes.NpgsqlDbType.Json, jsonString);
                
                // Add notification GUID
                command.Parameters.AddWithValue("notificationGuid", notificationGuid.Value);

                // Execute the command and get the inserted ID
                var insertedId = await command.ExecuteScalarAsync();
                return (long)insertedId;
            }
        }
    }

    // Method for dequeuing messages that takes a queue table name
    public async Task<List<IdAndMessageAndNotificationGuid>> DequeueMessages(string callingProcessorId, int numElements, string queueTableName)
    {
        List<IdAndMessageAndNotificationGuid> messages = new List<IdAndMessageAndNotificationGuid>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Use the new dynamic stored procedure with queue name parameter
            using (var command = new NpgsqlCommand(
                "SELECT * FROM queues.take_elements_for_processing(@queueName, @callingProcessorId, @numElements)",
                connection))
            {
                command.Parameters.AddWithValue("queueName", queueTableName);
                command.Parameters.AddWithValue("callingProcessorId", callingProcessorId);
                command.Parameters.AddWithValue("numElements", numElements);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var id = reader.GetInt64(0); // First column is id
                        var messageJson = reader.GetString(1); // Second column is message

                        // Third column is notification_guid (index 2)
                        var notificationGuid = reader.GetGuid(2);

                        messages.Add(new IdAndMessageAndNotificationGuid
                        {
                            Id = id,
                            Message = messageJson,
                            NotificationGuid = notificationGuid
                        });
                    }
                }
            }
        }
        return messages;
    }

    public async Task MarkMessageAsDone(Guid notification_guid, string processingResultText, string callingProcessorId, string queueName)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand("SELECT queues.mark_element_as_done(@queueName, @notification_guid, @processingResultText, @callingProcessorId)", connection))
            {
                // Add parameters for the function
                command.Parameters.AddWithValue("queueName", queueName);
                command.Parameters.AddWithValue("notification_guid", notification_guid);
                command.Parameters.AddWithValue("processingResultText", processingResultText);
                command.Parameters.AddWithValue("callingProcessorId", callingProcessorId);

                // Execute the command
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}