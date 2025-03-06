using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace MessageQueueAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageQueueController : ControllerBase
    {
        private readonly MessageQueueService _messageQueueService;

        public MessageQueueController(MessageQueueService messageQueueService)
        {
            _messageQueueService = messageQueueService;
        }
        public class MessageRequest
        {
            public string Message { get; set; }
        }

        [HttpPost("publish/{eventName}")]
        public async Task<IActionResult> PublishMessage([FromBody] JsonElement jsonElement, string eventName)
        {
            Console.WriteLine("MessageQueueController.PublishMessage()");

            string jsonString = jsonElement.GetRawText();

            long insertedId = await _messageQueueService.EnqueueMessage(jsonString, eventName);
            return Ok(new { Status = "Message published successfully", Id = insertedId });
        }

        // Endpoint to poll for new messages
        [HttpGet("poll")]
        public async Task<IActionResult> PollMessages()
        {
            var messages = await _messageQueueService.DequeueMessages("processor_1", 1);
            foreach (var (id, message) in messages)
            {
                // Pseudo-processing logic
                Console.WriteLine($"Processing Message ID: {id}, Message: {message}");

                // Simulate processing result
                string processingResultText = "Processed successfully";

                // Mark the message as done
                await _messageQueueService.MarkMessageAsDone(id, processingResultText, "processor_1");
            }
            return Ok(messages);
        }
    }
}