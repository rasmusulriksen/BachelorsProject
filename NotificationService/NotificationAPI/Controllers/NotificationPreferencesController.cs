// <copyright file="NotificationPreferencesController.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.NotificationAPI.Services.NotificationPreferencesService;

/// <summary>
/// Controller for managing notification preferences.
/// </summary>
[ApiController]
[Route("[controller]")]
public class NotificationPreferencesController : ControllerBase
{
    private readonly INotificationPreferencesService service;
    private readonly ILogFactory logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPreferencesController"/> class.
    /// </summary>
    /// <param name="service">The notification preferences service.</param>
    /// <param name="logger">The logger.</param>
    public NotificationPreferencesController(INotificationPreferencesService service, ILogFactory logger)
    {
        this.service = service;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a specific notification preference for a user. I.e. if the user wants to be notified in all owned cases.
    /// </summary>
    /// <param name="username">The username of the user to get the preference for.</param>
    /// <param name="preferenceToLookup">The preference to lookup.</param>
    /// <returns>The notification preference.</returns>
    [HttpGet("{username}/{preferenceToLookup}")]
    public async Task<IActionResult> GetByUsername(string username, string preferenceToLookup, [FromHeader(Name = "X-Tenant-Identifier")] string tenantIdentifier)
    {
        var preference = await this.service.Get1BoolByUsernameAsync(username, preferenceToLookup, tenantIdentifier);
        return this.Ok(preference);
    }
}
