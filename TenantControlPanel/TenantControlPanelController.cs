// <copyright file="TenantControlPanelController.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.TenantControlPanel;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Visma.Ims.Common.Abstractions.Logging;

/// <summary>
/// Controller for TenantControlPanel operations.
/// </summary>
[ApiController]
[Route("tenantcontrolpanel")]
public class TenantControlPanelController : ControllerBase
{
    private readonly ILogFactory logger;
    private readonly TenantControlPanelService tenantControlPanelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantControlPanelController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public TenantControlPanelController(ILogFactory logger, TenantControlPanelService tenantControlPanelService)
    {
        this.logger = logger;
        this.tenantControlPanelService = tenantControlPanelService;
    }

    [HttpPost("onboard")]
    public async Task<IActionResult> OnboardTenant([FromBody] OnboardTenantRequest request)
    {
        await this.tenantControlPanelService.OnboardTenant(request);
        return this.Ok($"Successfully onboarded tenant: {request.TenantIdentifier}");
    }

    [HttpGet("teardown")]
    public async Task<IActionResult> TeardownTenant([FromQuery] string tenantIdentifier)
    {
        await this.tenantControlPanelService.TeardownTenant(tenantIdentifier);
        return this.Ok($"Successfully teared down tenant: {tenantIdentifier}");
    }
}
