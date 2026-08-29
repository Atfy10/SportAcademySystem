using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Translations;

/// <summary>Arabic override for <see cref="PaymentType.Name"/> (e.g. "Cash" / "نقدي").</summary>
public class PaymentTypeTranslation : ITenantScoped
{
    public int PaymentTypeId { get; set; }
    public PaymentType PaymentType { get; set; } = null!;

    public string LangCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
