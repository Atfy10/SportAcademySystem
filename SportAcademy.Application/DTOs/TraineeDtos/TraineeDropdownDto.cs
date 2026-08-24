using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeDropdownDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public Gender Gender { get; init; }
    public List<TraineeSportSkillItemDto> SportSkills { get; init; } = [];
}

// Per-sport skill level for the trainee-vs-group eligibility check on the enrollment form -
// a trainee's skill varies by sport, so this can't be a single scalar on the dropdown item.
public record TraineeSportSkillItemDto(int SportId, SkillLevel SkillLevel);
