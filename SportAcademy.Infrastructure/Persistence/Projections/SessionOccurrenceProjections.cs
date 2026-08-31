using System.Linq.Expressions;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Projections;

/// <summary>
/// Hand-written translated projection for SessionOccurrence - see SportProjections for why.
/// Mirrors SessionOccurrenceMappingProfile's field-for-field logic exactly; only TraineeGroupName,
/// SportName and BranchName resolve through their own entity's translation table (CoachName stays
/// untouched - it's an employee's own name, not reference data with an Arabic translation).
/// </summary>
public static class SessionOccurrenceProjections
{
    public static Expression<Func<SessionOccurrence, SessionOccurrenceDto>> ToDto(string lang) => s => new SessionOccurrenceDto(
        s.Id,
        s.GroupSchedule!.TraineeGroup.Id,
        DateOnly.FromDateTime(s.StartDateTime),
        s.GroupSchedule!.TraineeGroup!.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? s.GroupSchedule!.TraineeGroup!.Name,
        s.GroupSchedule!.TraineeGroup!.Coach!.Sport!.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? s.GroupSchedule!.TraineeGroup!.Coach!.Sport!.Name,
        s.GroupSchedule!.TraineeGroup!.Coach.Employee!.FirstName + " " + s.GroupSchedule!.TraineeGroup!.Coach.Employee.LastName,
        s.GroupSchedule!.TraineeGroup!.Branch!.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? s.GroupSchedule!.TraineeGroup!.Branch!.Name,
        s.StartDateTime.ToString("HH:mm:ss"),
        s.GroupSchedule!.TraineeGroup!.DurationInMinutes,
        s.GroupSchedule!.TraineeGroup!.Enrollments.Count(e => e.IsActive),
        s.Attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present),
        s.Attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Late),
        s.Attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Absent));
}
