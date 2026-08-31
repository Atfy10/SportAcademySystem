using System.Linq.Expressions;
using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Projections;

/// <summary>
/// Hand-written translated projections for TraineeGroup - see SportProjections for why.
/// Mirrors TraineeGroupMappingProfile's existing field-for-field logic exactly. Name, SportName
/// and BranchName all resolve through their own entity's translation table (CoachName stays
/// untouched - it's an employee's own name, not reference data with an Arabic translation).
/// </summary>
public static class TraineeGroupProjections
{
    public static Expression<Func<TraineeGroup, TraineeGroupCardDto>> ToCardDto(string lang) => g => new TraineeGroupCardDto
    {
        Id = g.Id,
        Name = g.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? g.Name,
        SportName = g.Coach.Sport.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? g.Coach.Sport.Name,
        CoachName = g.Coach.Employee.FirstName,
        BranchName = g.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? g.Branch.Name,
        DurationInMinutes = g.DurationInMinutes,
        TraineesCount = g.Enrollments.Count,
        Schedules = g.GroupSchedules
            .Select(gs => new GroupSchedulesTimesDto { DayOfWeek = gs.Day, StartTime = gs.StartTime })
            .ToList(),
    };

    public static Expression<Func<TraineeGroup, ListTraineeGroupDto>> ToListDto(string lang) => g => new ListTraineeGroupDto(
        g.Id,
        g.Coach.Sport.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? g.Coach.Sport.Name,
        g.Coach.Employee.FirstName,
        g.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? g.Branch.Name,
        g.DurationInMinutes,
        g.Enrollments.Count,
        g.GroupSchedules
            .Select(gs => new GroupScheduleItemDto
            {
                DayOfWeek = gs.Day.ToString(),
                StartTime = gs.StartTime.ToString("HH:mm:ss"),
            })
            .ToList());

    public static Expression<Func<TraineeGroup, TraineeGroupDropdownDto>> ToDropdownDto(string lang) => g => new TraineeGroupDropdownDto(
        g.Id,
        g.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? g.Name,
        g.Coach.SportId,
        g.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? g.Branch.Name,
        g.Coach.Employee.FirstName,
        g.SkillLevel,
        g.Gender);
}
