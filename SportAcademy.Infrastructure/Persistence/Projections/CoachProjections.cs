using System.Linq.Expressions;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Projections;

/// <summary>
/// Hand-written translated projections for Coach - see SportProjections for why. Mirrors
/// CoachProfile's existing field-for-field logic exactly; only BranchName/SportName resolve
/// through their own entity's translation table.
/// </summary>
public static class CoachProjections
{
    public static Expression<Func<Coach, CoachCardDto>> ToCardDto(string lang) => c => new CoachCardDto(
        c.EmployeeId,
        c.Employee.FirstName,
        c.Employee.LastName,
        c.Employee.Position.ToString(),
        c.Employee.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? c.Employee.Branch.Name,
        c.Employee.Email.ToString(),
        c.Employee.IsWork,
        c.Employee.PhoneNumber,
        c.Employee.Address.ToString(),
        c.Employee.HireDate,
        c.TraineeGroups.SelectMany(tg => tg.Enrollments).Count(e => e.IsActive && !e.IsDeleted),
        c.SkillLevel,
        c.Sport.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? c.Sport.Name);

    public static Expression<Func<Coach, CoachDropdownItemDto>> ToDropdownDto(string lang) => c => new CoachDropdownItemDto
    {
        Id = c.EmployeeId,
        EmployeeFirstName = c.Employee.FirstName,
        EmployeeLastName = c.Employee.LastName,
        BranchId = c.Employee.BranchId,
        BranchName = c.Employee.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? c.Employee.Branch.Name,
        SportId = c.SportId,
        SkillLevel = c.SkillLevel,
    };
}
