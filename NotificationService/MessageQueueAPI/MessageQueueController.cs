using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Model;

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

        [HttpGet("poll/{eventName}")]
        public async Task<IActionResult> PollMessages(string eventName)
        {
            List<IdAndMessage> messages = await _messageQueueService.DequeueMessages("processor_1", 1, eventName);
            foreach (var message in messages)
            {
                // Pseudo-processing logic
                Console.WriteLine($"Processing Message ID: {message.Id}, Message: {message.Message}");

                // Simulate processing result
                string processingResultText = "Processed successfully";

                // Mark the message as done
                // await _messageQueueService.MarkMessageAsDone(id, processingResultText, "processor_1");
            }
            return Ok(messages);
        }

        [HttpGet("done/{id}")]
        public async Task<IActionResult> MarkMessageAsDone(long id)
        {
            string processingResultText = "Processed successfully";
            await _messageQueueService.MarkMessageAsDone(id, processingResultText, "processor_1");
            return Ok(new { Status = "Message marked as done" });
        }
    }
}