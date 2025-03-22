using System.Text.Json.Serialization;

/// <summary>
/// Represents the JSON data of an email activity.
/// </summary>
public class JsonData
    {
        /// <summary>
        /// Gets or sets the reference to the document record node.
        /// </summary>
        [JsonPropertyName("docRecordNodeRef")]
        public string DocRecordNodeRef { get; set; }

        /// <summary>
        /// Gets or sets the display name of the modifier.
        /// </summary>
        [JsonPropertyName("modifierDisplayName")]
        public string ModifierDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the modifier.
        /// </summary>
        [JsonPropertyName("modifier")]
        public string Modifier { get; set; }

        /// <summary>
        /// Gets or sets the ID of the case.
        /// </summary>
        [JsonPropertyName("caseId")]
        public string CaseId { get; set; }

        /// <summary>
        /// Gets or sets the title of the case.
        /// </summary>
        [JsonPropertyName("caseTitle")]
        public string CaseTitle { get; set; }

        /// <summary>
        /// Gets or sets the title of the document.
        /// </summary>
        [JsonPropertyName("docTitle")]
        public string DocTitle { get; set; }

        /// <summary>
        /// Gets or sets the title of the parent.
        /// </summary>
        [JsonPropertyName("parentTitle")]
        public string ParentTitle { get; set; }

        /// <summary>
        /// Gets or sets the type of the parent.
        /// </summary>
        [JsonPropertyName("parentType")]
        public string ParentType { get; set; }

        /// <summary>
        /// Gets or sets the reference to the parent.
        /// </summary>
        [JsonPropertyName("parentRef")]
        public string ParentRef { get; set; }
    }