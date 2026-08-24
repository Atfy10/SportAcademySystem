using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.CoachDtos;

public record CoachDropdownItemDto
{
    public int Id { get; init; }
    public string EmployeeFirstName { get; init; } = default!;
    public string EmployeeLastName { get; init; } = default!;
    public int BranchId { get; init; }
    public string BranchName { get; init; } = default!;
    public int SportId { get; init; }
    public SkillLevel SkillLevel { get; init; }
}
