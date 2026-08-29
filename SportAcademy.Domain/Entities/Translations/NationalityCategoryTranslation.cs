namespace SportAcademy.Domain.Entities.Translations;

/// <summary>
/// Arabic override for <see cref="NationalityCategory.Name"/>.
/// </summary>
/// <remarks>
/// Not <c>ITenantScoped</c> - NationalityCategory itself is a global lookup table shared across
/// every tenant, not a per-tenant one, so its translations are too.
/// </remarks>
public class NationalityCategoryTranslation
{
    public int NationalityCategoryId { get; set; }
    public NationalityCategory NationalityCategory { get; set; } = null!;

    public string LangCode { get; set; } = null!;

    public string Name { get; set; } = null!;
}
