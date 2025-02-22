using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotificationAPI.Model;

public class OutboundNotification
{
    public string ActivityType { get; set; }

    public JsonDocument JsonData { get; set; }

    public string UserId { get; set; }

    // Default constructor for deserialization
    public OutboundNotification() { }
}
