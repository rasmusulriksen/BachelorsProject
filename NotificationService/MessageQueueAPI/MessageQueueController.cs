// <copyright file="MessageQueueController.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controller doing message queue operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MessageQueueController : ControllerBase
    {
        private readonly IMessageQueueRepo messageQueueRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueController"/> class.
        /// </summary>
        /// <param name="messageQueueRepo">The message queue repository.</param>
        public MessageQueueController(IMessageQueueRepo messageQueueRepo)
        {
            this.messageQueueRepo = messageQueueRepo;
        }

        /// <summary>
        /// Publishes a message to the message queue.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="eventName">The event name.</param>
        /// <returns>Id of the inserted message</returns>
        [HttpPost("publish/{eventName}")]
        public async Task<IActionResult> PublishMessage([FromBody] NotificationMessage message, string eventName)
        {
            Console.WriteLine($"MessageQueueController.PublishMessage(): {eventName}");

            // Serialize the message object to JSON
            string jsonString = JsonSerializer.Serialize(message);

            long insertedId = await this.messageQueueRepo.EnqueueMessage(jsonString, eventName);

            return this.Ok(new { Status = "Message published successfully", Id = insertedId });
        }

        /// <summary>
        /// Polls messages from the message queue.
        /// </summary>
        /// <param name="count">The number of messages to poll.</param>
        /// <returns>The action result.</returns>
        [HttpGet("poll")]
        public async Task<IActionResult> PollMessages(int count = 1)
        {
            try
            {
                string referer = this.GetReferer();

                string queueTable = RefererToQueueTableMapper.GetQueueTableName(referer);

                Console.WriteLine($"PollMessages: Dequeuing from table '{queueTable}' for referer '{referer}'");

                List<IdAndMessage> messages = await this.messageQueueRepo.DequeueMessages(referer, count, queueTable);

                foreach (var message in messages)
                {
                    Console.WriteLine($"MessageQueueController.PollMessages(): {message.Id}, Message: {message.Message}");
                }

                return this.Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PollMessages: {ex.Message}");
                return this.BadRequest(new { Error = ex.Message });
            }
        }

        // Get the referer or return a default value
        private string GetReferer()
        {
            if (this.HttpContext.Request.Headers.TryGetValue("Referer", out var refererUrl))
            {
                return refererUrl.ToString();
            }

            throw new ArgumentException("Referer header not found in the request");
        }
    }

public class NotificationMessage
{
    public string ActivityType { get; set; }
    public JsonData JsonData { get; set; }
    public string UserName { get; set; }
    public string ToEmail { get; set; }
    public string FromEmail { get; set; }
}

public class JsonData
{
    public string ModifierDisplayName { get; set; }
    public string CaseId { get; set; }
    public string CaseTitle { get; set; }
    public string DocTitle { get; set; }
}
