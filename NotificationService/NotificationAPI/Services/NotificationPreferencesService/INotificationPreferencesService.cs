// <copyright file="INotificationPreferencesService.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.NotificationAPI.Services.NotificationPreferencesService;

using Visma.Ims.NotificationAPI.Model;

/// <summary>
/// Service for managing notification preferences.
/// </summary>
public interface INotificationPreferencesService
{
    /// <summary>
    /// Gets a specific property of the notification preference by username.
    /// </summary>
    /// <param name="username">The username of the user to get the preference for.</param>
    /// <param name="preferenceToLookup">The preference to lookup.</param>
    /// <returns>The notification preference.</returns>
    Task<bool> Get1BoolByUsernameAsync(string username, string preferenceToLookup);

    /// <summary>
    /// Gets the entire notification preference by username.
    /// </summary>
    /// <param name="username">The username of the user to get the preference for.</param>
    /// <returns>The notification preference.</returns>
    Task<NotificationPreference> GetNotificationPreferenceObjectByUsernameAsync(string username);
}
