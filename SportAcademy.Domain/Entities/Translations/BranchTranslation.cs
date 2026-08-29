using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Translations;

/// <summary>Arabic override for <see cref="Branch.Name"/>, <see cref="Branch.City"/> and <see cref="Branch.Country"/>.</summary>
public class BranchTranslation : ITenantScoped
{
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public string LangCode { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? City { get; set; }
    public string? Country { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
