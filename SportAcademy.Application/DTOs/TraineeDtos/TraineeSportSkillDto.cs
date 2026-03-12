using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeSportSkillDto
{
    public string SportName { get; init; } = null!;
    public SkillLevel SkillLevel { get; init; }
}
