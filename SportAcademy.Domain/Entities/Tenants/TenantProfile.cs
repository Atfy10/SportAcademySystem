namespace SportAcademy.Domain.Entities.Tenants;

public class TenantProfile
{
    public Guid TenantId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegistration { get; set; }
    public string? Description { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
