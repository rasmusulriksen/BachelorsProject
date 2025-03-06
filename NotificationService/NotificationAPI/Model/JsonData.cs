using System.Text.Json.Serialization;

public class JsonData
{
    [JsonPropertyName("docRecordNodeRef")]
    public string DocRecordNodeRef { get; set; }

    [JsonPropertyName("modifierDisplayName")]
    public string ModifierDisplayName { get; set; }

    [JsonPropertyName("modifier")]
    public string Modifier { get; set; }

    [JsonPropertyName("caseId")]
    public string CaseId { get; set; }

    [JsonPropertyName("caseTitle")]
    public string CaseTitle { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("parentTitle")]
    public string ParentTitle { get; set; }

    [JsonPropertyName("parentType")]
    public string ParentType { get; set; }

    [JsonPropertyName("parentRef")]
    public string ParentRef { get; set; }

    public JsonData()
    { 
    }

}