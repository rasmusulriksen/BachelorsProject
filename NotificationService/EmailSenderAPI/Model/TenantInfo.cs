using System.Text.Json.Serialization;

public class TenantInfo
{
    [JsonPropertyName("tenantIdentifier")]
    public string TenantIdentifier { get; set; }

    [JsonPropertyName("tenantUrl")]
    public string TenantUrl { get; set; }
} 