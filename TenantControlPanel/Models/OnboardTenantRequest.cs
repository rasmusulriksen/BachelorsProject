public class OnboardTenantRequest
{
    /// <summary>
    /// The identifier of the tenant. I.e. "vgt" (Vestjysk Gymnasium Tarm)
    /// </summary>
    public string TenantIdentifier { get; set; }

    /// <summary>
    /// The name of the tenant. I.e. "Vestjysk Gymnasium Tarm"
    /// </summary>
    public string TenantName { get; set; }

    /// <summary>
    /// The tier of the tenant: "Small", "Medium", "Large"
    /// </summary>
    public string TenantTier { get; set; }
}

public static class TenantTier
{
    public const string Small = "Small";
    public const string Medium = "Medium";
    public const string Large = "Large";
}
