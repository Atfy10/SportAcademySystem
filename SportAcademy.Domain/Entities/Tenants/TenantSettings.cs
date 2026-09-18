namespace SportAcademy.Domain.Entities.Tenants;

public class TenantSettings
{
    public Guid TenantId { get; set; }
    public string TimeZone { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string DateFormat { get; set; } = null!;
    public string TimeFormat { get; set; } = null!;
    public string Currency { get; set; } = null!;

    /// <summary>ISO 3166-1 alpha-2 country code. Drives phone-number and National ID format
    /// rules (see SportAcademy.Application.Common.Regional.CountryRegionalRegistry) - the same
    /// per-tenant, not per-branch, shape as every other field on this entity.</summary>
    public string Country { get; set; } = "KW";

    public Tenant Tenant { get; set; } = null!;
}
