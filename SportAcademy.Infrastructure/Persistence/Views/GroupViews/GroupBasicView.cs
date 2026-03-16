using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.Views.Interfaces;

namespace SportAcademy.Infrastructure.Persistence.Views.GroupViews;

public class GroupBasicView : IModelView
{
    public int TraineeGroupId { get; set; }

    public SkillLevel SkillLevel { get; set; }

    public int MaximumCapacity { get; set; }

    public int DurationInMinutes { get; set; }

    public Gender Gender { get; set; }

    public string BranchName { get; set; } = null!;

    public string CoachName { get; set; } = null!;
}
