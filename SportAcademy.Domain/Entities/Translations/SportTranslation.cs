using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Translations;

/// <summary>
/// Arabic (or any future language) override for <see cref="Sport.Name"/> and
/// <see cref="Sport.Description"/>.
/// </summary>
/// <remarks>
/// Composite key (SportId, LangCode) rather than a NameAr column: adding a third language is
/// then a matter of inserting rows, not a schema migration, and Sport.Name stays the
/// single source of truth plus the fallback when no row exists for the request language.
/// </remarks>
public class SportTranslation : ITenantScoped
{
    public int SportId { get; set; }
    public Sport Sport { get; set; } = null!;

    /// <summary>Neutral two-letter code, e.g. "ar". See CurrentLanguageProvider.Supported.</summary>
    public string LangCode { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
