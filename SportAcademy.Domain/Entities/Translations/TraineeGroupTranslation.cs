using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Translations;

/// <summary>Arabic override for <see cref="TraineeGroup.Name"/> (e.g. "Beginners A" / "المبتدئون أ").</summary>
public class TraineeGroupTranslation : ITenantScoped
{
    public int TraineeGroupId { get; set; }
    public TraineeGroup TraineeGroup { get; set; } = null!;

    public string LangCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
