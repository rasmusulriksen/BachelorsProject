// ------------------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>
// ------------------------------------------------------------------------------

namespace Visma.Ims.NotificationAPI.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SimpleInjector;
using Visma.Ims.Common.Abstractions.Logging;
using Visma.Ims.Common.Infrastructure.DependencyInjection;
using Visma.Ims.Common.Infrastructure.Logging;
using Visma.Ims.NotificationAPI.Repositories;
using Visma.Ims.NotificationAPI.Services;
using Visma.Ims.NotificationAPI.Services.NotificationPreferencesService;
using Visma.Ims.NotificationAPI.Services.NotificationService;

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

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        // make sure that we can read the ConnectionString from the appsettings.json file
        var connectionString = configuration.GetConnectionString("ConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("ConnectionString is not set in the appsettings.json file.");
        }

    }

    /// <inheritdoc/>
    protected override void RegisterDependencies(Container container)
    {
        container.RegisterSingleton<INotificationService, NotificationService>();
        container.RegisterSingleton<INotificationRepository, NotificationRepository>();
        container.RegisterSingleton<INotificationPreferencesService, NotificationPreferencesService>();
        container.RegisterSingleton<INotificationPreferencesRepository, NotificationPreferencesRepository>();
    }

    /// <inheritdoc/>
    protected override void ConfigureHttpClients(IServiceCollection services)
    {
        services.AddHttpClient("MessageQueueClient", client =>
        {
            client.DefaultRequestHeaders.Referrer = new Uri("http://localhost:5258");
        });

        services.AddHttpClient("InAppNotificationClient", client =>
        {
            client.DefaultRequestHeaders.Referrer = new Uri("http://localhost:5258");
        });
    }
}
