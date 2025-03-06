using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

public class MessageQueueService
{
    private readonly string _connectionString;

    public MessageQueueService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> EnqueueMessage(string jsonString, string eventName)
    {
        Console.WriteLine("MessageQueueService.EnqueueMessage()");

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string queueToHit = EventNameToDbTableMapper.GetDbTableForEventName(eventName);

            using (var command = new NpgsqlCommand($"SELECT * FROM {queueToHit}_insert_into_queue(@message)", connection))

            {
                // Add the parameter as a JSON type
                command.Parameters.AddWithValue("message", NpgsqlTypes.NpgsqlDbType.Json, jsonString);

                // Execute the command and get the inserted ID
                var insertedId = await command.ExecuteScalarAsync();
                return (long)insertedId;
            }
        }
    }

    public async Task<List<(long Id, string Message)>> DequeueMessages(string callingProcessorId, int numElements)
    {
        var messages = new List<(long Id, string Message)>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("SELECT * FROM queues.notifications_take_elements_for_processing(@callingProcessorId, @numElements)", connection))
            {
                command.Parameters.AddWithValue("callingProcessorId", callingProcessorId);
                command.Parameters.AddWithValue("numElements", numElements);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var id = reader.GetInt64(0); // Assuming the first column is id
                        var messageJson = reader.GetString(1); // Assuming the second column is message
                        messages.Add((id, messageJson));
                    }
                }
            }
        }
        return messages;
    }

    public async Task MarkMessageAsDone(long elementId, string processingResultText, string callingProcessorId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new NpgsqlCommand("SELECT queues.notifications_mark_element_as_done(@elementId, @processingResultText, @callingProcessorId)", connection))
            {
                // Add parameters for the function
                command.Parameters.AddWithValue("elementId", elementId);
                command.Parameters.AddWithValue("processingResultText", processingResultText);
                command.Parameters.AddWithValue("callingProcessorId", callingProcessorId);

                // Execute the command
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}