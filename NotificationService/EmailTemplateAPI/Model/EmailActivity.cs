using System.Text.Json.Serialization;

/// <summary>
/// Represents an email activity.
/// </summary>
public class EmailActivity
    {
        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        [JsonPropertyName("ActivityType")]
        public string ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the JSON data of the activity.
        /// </summary>
        [JsonPropertyName("JsonData")]
        public JsonData JsonData { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        [JsonPropertyName("UserName")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the recipient.
        /// </summary>
        [JsonPropertyName("ToEmail")]
        public string ToEmail { get; set; }

        /// <summary>
        /// Gets or sets the email address of the sender.
        /// </summary>
        [JsonPropertyName("FromEmail")]
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether links are enabled.
        /// </summary>
        [JsonPropertyName("LinksEnabled")]
        public bool LinksEnabled { get; set; }
    }
    