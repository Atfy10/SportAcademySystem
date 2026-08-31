using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Translations;

/// <summary>
/// Arabic (or any future language) override for <see cref="Family.Name"/> and
/// <see cref="Family.GuardianName"/>. See SportTranslation for why this is a side table
/// rather than *Ar columns.
/// </summary>
public class FamilyTranslation : ITenantScoped
{
    public int FamilyId { get; set; }
    public Family Family { get; set; } = null!;

    /// <summary>Neutral two-letter code, e.g. "ar". See CurrentLanguageProvider.Supported.</summary>
    public string LangCode { get; set; } = null!;

    public string? Name { get; set; }
    public string? GuardianName { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
