// <copyright file="EventNameToEmailTemplateNameMapper.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

/// <summary>
/// Maps event names to email template names.
/// </summary>
public static class EventNameToEmailTemplateNameMapper
{
    /// <summary>
    /// The name of the email template for when a document is uploaded to a case.
    /// </summary>
    public const string DocumentUploadedToCase = "DocumentUploadedToCase";

    /// <summary>
    /// Gets the email template name from the event name.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>The name of the email template.</returns>
    public static string GetEmailTemplateNameFromEventName(string eventName)
    {
        return eventName switch
        {
            "dk.openesdh.case.document-upload" => DocumentUploadedToCase,
            _ => throw new ArgumentException($"Unknown event name: {eventName}")
        };
    }
}
