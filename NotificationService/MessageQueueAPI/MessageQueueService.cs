using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Model;
using System;
using MessageQueueAPI;

public class MessageQueueService
{
    private readonly string _connectionString;

    public MessageQueueService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> EnqueueMessage(string jsonString, string eventName, Guid? notificationGuid = null)
    {
        Console.WriteLine("MessageQueueService.EnqueueMessage()");

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string queueToHit = EventNameToDbTableMapper.GetDbTableForEventName(eventName);
            string sqlCommand;
            
            // If notificationGuid is provided (for downstream queues), use it
            if (notificationGuid.HasValue)
            {
                sqlCommand = $"SELECT * FROM {queueToHit}_insert_into_queue(@message, @notificationGuid)";
            }
            else 
            {
                // For the first queue (notifications), let the database generate the GUID
                sqlCommand = $"SELECT * FROM {queueToHit}_insert_into_queue(@message)";
            }

            using (var command = new NpgsqlCommand(sqlCommand, connection))
            {
                // Add the parameter as a JSON type
                command.Parameters.AddWithValue("message", NpgsqlTypes.NpgsqlDbType.Json, jsonString);
                
                // Add notification GUID if provided
                if (notificationGuid.HasValue)
                {
                    command.Parameters.AddWithValue("notificationGuid", notificationGuid.Value);
                }

                // Execute the command and get the inserted ID
                var insertedId = await command.ExecuteScalarAsync();
                return (long)insertedId;
            }
        }
    }

    // Method for dequeuing messages that takes a queue table name
    public async Task<List<IdAndMessage>> DequeueMessages(string callingProcessorId, int numElements, string queueTableName)
    {
        List<IdAndMessage> messages = new List<IdAndMessage>();
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
                        
                        messages.Add(new IdAndMessage 
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