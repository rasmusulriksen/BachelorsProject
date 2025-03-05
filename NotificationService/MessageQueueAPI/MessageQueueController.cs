using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage([FromBody] MessageRequest jsonString)
        {
            Console.WriteLine("MessageQueueController.PublishMessage()");
            if (jsonString == null || string.IsNullOrWhiteSpace(jsonString.Message))
            {
                return BadRequest(new { Status = "Message cannot be empty" });
            }

            long insertedId = await _messageQueueService.EnqueueMessage(jsonString.Message);
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