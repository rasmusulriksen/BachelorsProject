using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Model;

namespace MessageQueueAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageQueueController : ControllerBase
    {
        private readonly IMessageQueueRepo _messageQueueRepo;

        public MessageQueueController(IMessageQueueRepo messageQueueRepo)
        {
            _messageQueueRepo = messageQueueRepo;
        }

        public class MessageRequest
        {
            public string Message { get; set; }
        }

        [HttpPost("publish/{eventName}")]
        public async Task<IActionResult> PublishMessage([FromBody] JsonElement payload, string eventName)
        {
            Console.WriteLine($"MessageQueueController.PublishMessage(): {eventName}");

            // Convert to string to preserve the original format
            string jsonString = JsonSerializer.Serialize(payload);

            // Try to extract GUID
            Guid? notificationGuid = null;
            try
            {
                // Check if notificationGuid property exists and try to parse it
                if (payload.TryGetProperty("notificationGuid", out JsonElement guidElement))
                {
                    string guidString = guidElement.GetString();
                    if (!string.IsNullOrEmpty(guidString))
                    {
                        notificationGuid = Guid.Parse(guidString);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MessageQueueController.PublishMessage(): {ex.Message}");
                return BadRequest(new { Error = ex.Message });
            }

            Guid insertedGuid = await _messageQueueRepo.EnqueueMessage(jsonString, eventName, notificationGuid);

            return Ok(new
            {
                Status = "Message published successfully",
                NotificationGuid = insertedGuid
            });
        }

        [HttpGet("poll")]
        public async Task<IActionResult> PollMessages(int count = 1)
        {
            try
            {
                string referer = GetReferer();

                string queueTable = RefererToQueueTableMapper.GetQueueTableName(referer);

                Console.WriteLine($"PollMessages: Dequeuing from table '{queueTable}' for referer '{referer}'");

                List<QueueMessage> messages = await _messageQueueRepo.DequeueMessages(referer, count, queueTable);

                foreach (var message in messages)
                {
                    Console.WriteLine($"MessageQueueController.PollMessages(): {message.NotificationGuid}, Message: {message.Message}");
                }

                return Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PollMessages: {ex.Message}");
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("done/{notificationGuid}")]
        public async Task<IActionResult> MarkMessageAsDone(Guid notificationGuid)
        {
            try
            {
                // Get the referer header
                string referer = GetReferer();

                // Get queue table name directly from referer
                string queueTable = RefererToQueueTableMapper.GetQueueTableName(referer);

                // Log for debugging - Include full details to verify the correct table name
                Console.WriteLine($"MarkMessageAsDone: Using table '{queueTable}' for referer '{referer}' and guid '{notificationGuid}'");

                await _messageQueueRepo.MarkMessageAsDone(notificationGuid, "success", referer, queueTable);
                return Ok(new { Status = "Message marked as done" });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ArgumentException in MarkMessageAsDone: {ex.Message}");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MarkMessageAsDone: {ex.Message}");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // Get the referer or return a default value
        private string GetReferer()
        {
            if (HttpContext.Request.Headers.TryGetValue("Referer", out var refererUrl))
            {
                return refererUrl.ToString();
            }
            
            throw new ArgumentException("Referer header not found in the request");

        }
    }
}