// <copyright file="MessageQueueController.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationService.MessageQueueAPI;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.NotificationService.MessageQueueAPI.Model;

/// <summary>
/// Controller doing message queue operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MessageQueueController : ControllerBase
{
    private readonly IMessageQueueRepo messageQueueRepo;
    private readonly ILogFactory logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageQueueController"/> class.
    /// </summary>
    /// <param name="messageQueueRepo">The message queue repository.</param>
    /// <param name="logger">The logger factory for logging.</param>
    public MessageQueueController(IMessageQueueRepo messageQueueRepo, ILogFactory logger)
    {
        this.messageQueueRepo = messageQueueRepo;
        this.logger = logger;
    }

    /// <summary>
    /// Publishes a message to the message queue.
    /// </summary>
    /// <param name="message">The message to publish as a JSON object.</param>
    /// <param name="eventName">The event name.</param>
    /// <returns>Id of the inserted message</returns>
    [HttpPost("publish/{eventName}")]
    public async Task<IActionResult> PublishMessage([FromBody] JObject message, string eventName)
    {
        this.logger.Log().Information($"MessageQueueController.PublishMessage(): {eventName}");

        long insertedId = await this.messageQueueRepo.EnqueueMessage(message, eventName);

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

            IEnumerable<IdAndJObject> messages = await this.messageQueueRepo.DequeueMessages(referer, count);

            this.logger.Log().Information($"MessageQueueController.PollMessages(): {messages.Count()} messages polled");

            return this.Ok(messages);
        }
        catch (Exception ex)
        {
            this.logger.Log().Error(ex, "Error in PollMessages");
            return this.BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Mark a message as done.
    /// </summary>
    /// <param name="id">The id of the message to mark as done.</param>
    /// <returns>The action result.</returns>
    [HttpPost("done/{id}")]
    public async Task<IActionResult> MarkMessageAsDone(long id)
    {
        string referer = this.GetReferer();

        await this.messageQueueRepo.MarkMessageAsDone(id, referer);
        return this.Ok(new { Status = "Message marked as done" });
    }

    private string GetReferer()
    {
        if (this.HttpContext.Request.Headers.TryGetValue("Referer", out var refererUrl))
        {
            return refererUrl.ToString();
        }

        throw new ArgumentException("Referer header not found in the request");
    }
}
