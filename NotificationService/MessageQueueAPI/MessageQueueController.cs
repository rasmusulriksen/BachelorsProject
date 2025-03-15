using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using MessageQueueAPI;

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
            Console.WriteLine($"MessageQueueController.PublishMessage(): {eventName}");

            string jsonString = jsonElement.GetRawText();

            long insertedId = await _messageQueueService.EnqueueMessage(jsonString, eventName);
            return Ok(new { Status = "Message published successfully", Id = insertedId });
        }

        [HttpGet("poll")]
        public async Task<IActionResult> PollMessages(int count = 1)
        {
            try
            {
                // Get the referer header
                string referer = GetRefererOrDefault();
                
                // Get queue table name directly from referer
                string queueTable = RefererToQueueTableMapper.GetQueueTableName(referer);
                
                // Log for debugging - Include full details to verify the correct table name
                Console.WriteLine($"PollMessages: Dequeuing from table '{queueTable}' for referer '{referer}'");
                
                // Dequeue messages
                List<IdAndMessageAndNotificationGuid> messages = await _messageQueueService.DequeueMessages(referer, count, queueTable);
                
                foreach (var message in messages)
                {
                    Console.WriteLine($"MessageQueueController.PollMessages(): {message.Id}, Message: {message.Message}");
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
                string referer = GetRefererOrDefault();
                
                // Get queue table name directly from referer
                string queueTable = RefererToQueueTableMapper.GetQueueTableName(referer);
                
                // Log for debugging - Include full details to verify the correct table name
                Console.WriteLine($"MarkMessageAsDone: Using table '{queueTable}' for referer '{referer}' and guid '{notificationGuid}'");

                await _messageQueueService.MarkMessageAsDone(notificationGuid, "success", referer, queueTable);
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
        private string GetRefererOrDefault()
        {
            if (HttpContext.Request.Headers.TryGetValue("Referer", out var refererValues))
            {
                return refererValues.ToString();
            }
            
            // For testing purposes or when no referer is provided
            return "http://localhost:5258"; // Default to NotificationAPI
        }
    }
}