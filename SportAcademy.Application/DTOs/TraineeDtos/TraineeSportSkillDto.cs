namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeSportSkillDto
{
    public string SportName { get; init; } = null!;
    public string SkillLevel { get; init; } = null!;
}
