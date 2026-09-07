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

    // Set true the first time the tenant Owner submits the post-invite "complete your academy
    // profile" onboarding step (see CompleteTenantSetupCommand) - drives whether the frontend
    // shows that wizard after AcceptInvitation instead of going straight to the dashboard.
    // Backfilled to true for every tenant that already existed before this flag was introduced
    // (see the AddTenantSetupCompleteFlag migration) so they are never retroactively gated.
    public bool IsSetupComplete { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
