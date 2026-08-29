using System.Linq.Expressions;
using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Projections;

/// <summary>
/// Hand-written translated projections for TraineeGroup - see SportProjections for why.
/// Mirrors TraineeGroupMappingProfile's existing field-for-field logic exactly, changing only
/// Name to resolve through the translation table; SportName/CoachName/BranchName stay untouched
/// (Sport/Employee/Branch's own translation is a separate concern for a later pass).
/// </summary>
public static class TraineeGroupProjections
{
    public static Expression<Func<TraineeGroup, TraineeGroupCardDto>> ToCardDto(string lang) => g => new TraineeGroupCardDto
    {
        Id = g.Id,
        Name = g.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? g.Name,
        SportName = g.Coach.Sport.Name,
        CoachName = g.Coach.Employee.FirstName,
        BranchName = g.Branch.Name,
        DurationInMinutes = g.DurationInMinutes,
        TraineesCount = g.Enrollments.Count,
        Schedules = g.GroupSchedules
            .Select(gs => new GroupSchedulesTimesDto { DayOfWeek = gs.Day, StartTime = gs.StartTime })
            .ToList(),
    };

    public static Expression<Func<TraineeGroup, TraineeGroupDropdownDto>> ToDropdownDto(string lang) => g => new TraineeGroupDropdownDto(
        g.Id,
        g.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? g.Name,
        g.Coach.SportId,
        g.Branch.Name,
        g.Coach.Employee.FirstName,
        g.SkillLevel,
        g.Gender);
}
