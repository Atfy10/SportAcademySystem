namespace SportAcademy.Domain.Entities.Tenants;

public class TenantSettings
{
    public Guid TenantId { get; set; }
    public string TimeZone { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string DateFormat { get; set; } = null!;
    public string TimeFormat { get; set; } = null!;
    public string Currency { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
