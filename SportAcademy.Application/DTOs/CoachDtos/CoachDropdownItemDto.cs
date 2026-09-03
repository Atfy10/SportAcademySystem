using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.CoachDtos;

public record CoachDropdownItemDto
{
    public int Id { get; init; }
    public string EmployeeFirstName { get; init; } = default!;
    public string EmployeeLastName { get; init; } = default!;
    // Employment branch (HR/payroll) - kept for display, but NOT the set of branches this coach
    // can be assigned to teach at. Use BranchIds for that.
    public int BranchId { get; init; }
    public string BranchName { get; init; } = default!;
    public int SportId { get; init; }
    public SkillLevel SkillLevel { get; init; }
    // Branches this coach is authorized to train at (CoachBranchAccess) - a group's coach picker
    // should only offer coaches whose BranchIds contains the group's chosen branch.
    public List<int> BranchIds { get; init; } = [];
}
