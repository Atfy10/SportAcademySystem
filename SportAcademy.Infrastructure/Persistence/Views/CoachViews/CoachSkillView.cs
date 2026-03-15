using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Views.CoachViews;

public class CoachSkillView
{
    public int Id { get; set; }
    public SkillLevel SkillLevel { get; set; }
    public string SportName { get; set; } = null!;
}
