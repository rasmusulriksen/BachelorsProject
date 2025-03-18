// ------------------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>
// ------------------------------------------------------------------------------

namespace Visma.Ims.NotificationService.MessageQueueAPI.Configuration;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.Common.Infrastructure.DependencyInjection;
using Visma.Ims.Common.Infrastructure.Logging;
using Visma.Ims.NotificationService.MessageQueueAPI;

/// <summary>
/// Represents the dependency injection configuration for the application.
/// </summary>
public class DependencyInjection(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    : DependencyInjectionBase(services, configuration, env)
{
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    // This is created explicitly, because the DI configuration is not available at this point.
    // Do not use this elsewhere, use the proper DI mechanism.
    private readonly ILogFactory logger = new SerilogLogFactory<DependencyInjection>();
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

    private string connectionString = null!;

    /// <inheritdoc/>
    protected override void LoadConfigurations(IConfiguration configuration)
    {
        this.connectionString = configuration.GetConnectionString("MessageQueueDb")
            ?? throw new InvalidOperationException("MessageQueueDb connection string not found in configuration");

        this.logger.Log().Information("Loaded MessageQueueDb connection string");
    }

    /// <inheritdoc/>
    protected override void RegisterDependencies(Container container)
    {
        // Register the MessageQueueRepo with its connection string
        container.RegisterInstance<IMessageQueueRepo>(new MessageQueueRepo(this.connectionString, this.logger));

        this.logger.Log().Information("Registered MessageQueueRepo with connection string");
    }

    /// <inheritdoc/>
    protected override void ConfigureHttpClients(IServiceCollection services)
    {
        // optional, can be left out.
        // load http clients here, if any is used. Remember to set correlation
    }
}
