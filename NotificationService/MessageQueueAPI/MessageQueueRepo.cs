using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Model;
using System;
using MessageQueueAPI;

public interface IMessageQueueRepo
{
    Task<Guid> EnqueueMessage(string jsonString, string eventName, Guid? notificationGuid = null);
    Task<List<QueueMessage>> DequeueMessages(string callingProcessorId, int numElements, string queueTableName);
    Task MarkMessageAsDone(Guid notification_guid, string processingResultText, string callingProcessorId, string queueName);
}

public class MessageQueueRepo : IMessageQueueRepo
{
    private readonly string _connectionString;

    public MessageQueueRepo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Guid> EnqueueMessage(string jsonString, string eventName, Guid? notificationGuid = null)
    {
        Console.WriteLine("MessageQueueRepo.EnqueueMessage()");

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string queueName = EventNameToDbTableMapper.GetDbTableForEventName(eventName);

            string sqlCommand = "SELECT queues.insert_into_queue(@queueName, @message, @notificationGuid)";

            using (var command = new NpgsqlCommand(sqlCommand, connection))
            {
                command.Parameters.AddWithValue("queueName", queueName);
                command.Parameters.AddWithValue("message", NpgsqlTypes.NpgsqlDbType.Json, jsonString);
                command.Parameters.AddWithValue("notificationGuid", notificationGuid.Value);

                // Execute the command and get the inserted GUID
                var insertedGuid = await command.ExecuteScalarAsync();
                return (Guid)insertedGuid;
            }
        }
    }

    public async Task<List<QueueMessage>> DequeueMessages(string callingProcessorId, int numElements, string queueTableName)
    {
        List<QueueMessage> messages = new List<QueueMessage>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

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
                        var messageJson = reader.GetString(0); // First column is message (index 0)

                        var notificationGuid = reader.GetGuid(1); // Second column is notification_guid (index 1)

                        messages.Add(new QueueMessage
                        {
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