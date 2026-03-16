using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.Views.Interfaces;

namespace SportAcademy.Infrastructure.Persistence.Views.CoachViews
{
    public class CoachSkillView : IModelView
    {
        public int Id { get; set; }
        public SkillLevel SkillLevel { get; set; }
        public string SportName { get; set; } = null!;
    }
}