using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.CoachDtos
{
    public class CoachSkillDto
    {
        public int Id { get; set; }
        public SkillLevel SkillLevel { get; set; }
        public string SportName { get; set; } = null!;
    }
}
