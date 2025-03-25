// ------------------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>
// ------------------------------------------------------------------------------

namespace Visma.Ims.TenantControlPanel.Configuration;

using SimpleInjector;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.Common.Infrastructure.DependencyInjection;
using Visma.Ims.Common.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

/// <summary>
/// Represents the dependency injection configuration for the application.
/// </summary>
public class DependencyInjection(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    : DependencyInjectionBase(services, configuration, environment)
{
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    // This is created explicitly, because the DI configuration is not available at this point.
    // Do not use this elsewhere, use the proper DI mechanism.
    private readonly ILogFactory logger = new SerilogLogFactory<DependencyInjection>();
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

    /// <inheritdoc/>
    protected override void LoadConfigurations(IConfiguration configuration)
    {
        // fetch required data to register dependencies in this class, from the IConfiguration object.
    }

    /// <inheritdoc/>
    protected override void RegisterDependencies(Container container)
    {
        // Register the TenantControlPanelService
        container.Register<TenantControlPanelService>(Lifestyle.Scoped);
    }

    /// <inheritdoc/>
    protected override void ConfigureHttpClients(IServiceCollection services)
    {
        // optional, can be left out.
        // load http clients here, if any is used. Remember to set correlation
    }
}
