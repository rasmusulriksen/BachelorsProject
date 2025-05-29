// ------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>
// ------------------------------------------------------------------------------

namespace Visma.Ims.NotificationAPI;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SimpleInjector;
using Visma.Ims.NotificationAPI.Configuration;
using Visma.Ims.Common.Infrastructure;

/// <summary>
/// Startup class for RecieverService.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Startup"/> class.
/// </remarks>
/// <param name="configuration">Configuration root.</param>
/// <param name="env">Hostinf envrionment.</param>
public class Startup(IConfiguration configuration, IWebHostEnvironment env)
: StartupBase(configuration, env)
{
    /// <inheritdoc/>
    protected override Container GetDependencyInjectionContainer(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        return new DependencyInjection(services, configuration, env).GetDiContainer();
    }

    /// <inheritdoc/>
    protected override void ConfigureHealthChecks(
        IHealthChecksBuilder builder,
        Container container)
    {
        // Add healthchecks here.
    }
}
