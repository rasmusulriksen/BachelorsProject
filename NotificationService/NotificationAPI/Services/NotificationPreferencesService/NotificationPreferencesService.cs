// <copyright file="NotificationPreferencesService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Services.NotificationPreferencesService;

using Visma.Ims.NotificationAPI.Model;
using Visma.Ims.NotificationAPI.Repositories;

/// <summary>
/// Service for managing notification preferences.
/// </summary>
public class NotificationPreferencesService : INotificationPreferencesService
{
    private readonly INotificationPreferencesRepository repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPreferencesService"/> class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    public NotificationPreferencesService(INotificationPreferencesRepository repository)
    {
        this.repository = repository;
    }

    /// <summary>
    /// Gets a specific property of the notification preference by username.
    /// </summary>
    /// <param name="username">The username of the user to get the preference for.</param>
    /// <param name="preferenceToLookup">The preference to lookup.</param>
    /// <returns>The notification preference.</returns>
    public async Task<bool> Get1BoolByUsernameAsync(string username, string preferenceToLookup)
    {
        NotificationPreference preference = await this.repository.GetByUsernameAsync(username);

        // Try and get the preferenceToLookup value from preference. Dont hardcode it.
        // Also, if preferenceToLookup = "caseOwner", I want it to successfully retrieve the preference.CaseOwner value
        // This means that it must be case agnostic
        var property = preference.GetType().GetProperty(preferenceToLookup);
        if (property != null)
        {
            return (bool)property.GetValue(preference);
        }

        return false;
    }

    /// <summary>
    /// Gets the entire notification preference by username.
    /// </summary>
    /// <param name="username">The username of the user to get the preference for.</param>
    /// <returns>The notification preference.</returns>
    public async Task<NotificationPreference> GetNotificationPreferenceObjectByUsernameAsync(string username)
    {
        return await this.repository.GetByUsernameAsync(username);
    }
}
