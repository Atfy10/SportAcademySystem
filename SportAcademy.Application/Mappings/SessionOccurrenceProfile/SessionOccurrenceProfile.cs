using SportAcademy.Application.Commands.SessionOccurrenceCommands.CreateSessionOccurrence;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.UpdateSessionOccurrence;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings.SessionOccurrenceProfile
{
    public class SessionOccurrenceMappingProfile : AutoMapper.Profile
    {
        public SessionOccurrenceMappingProfile()
        {
            // .ForCtorParam() here, not .ConstructUsing() - ConstructUsing opts a mapping out of
            // AutoMapper's LINQ expression-tree translation, which ProjectTo relies on to turn
            // this into a SQL projection (same root cause as the Coach dropdown fix). Plain
            // .ForMember() doesn't work either: SessionOccurrenceDto is a positional record with
            // no parameterless constructor, so AutoMapper must be told how to fill constructor
            // parameters explicitly via ForCtorParam.
            CreateMap<SessionOccurrence, SessionOccurrenceDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("TraineeGroupId", opt => opt.MapFrom(src => src.GroupSchedule!.TraineeGroup.Id))
                .ForCtorParam("Date", opt => opt.MapFrom(src => DateOnly.FromDateTime(src.StartDateTime)))
                .ForCtorParam("TraineeGroupName", opt => opt.MapFrom(src => src.GroupSchedule!.TraineeGroup!.Name))
                .ForCtorParam("SportName", opt => opt.MapFrom(src => src.GroupSchedule!.TraineeGroup!.Coach!.Sport!.Name))
                .ForCtorParam("CoachName", opt => opt.MapFrom(src => src.GroupSchedule!.TraineeGroup!.Coach.Employee!.FirstName + " " + src.GroupSchedule!.TraineeGroup!.Coach.Employee.LastName))
                .ForCtorParam("BranchName", opt => opt.MapFrom(src => src.GroupSchedule!.TraineeGroup!.Branch!.Name))
                .ForCtorParam("StartTime", opt => opt.MapFrom(src => src.StartDateTime.ToString("HH:mm:ss")))
                .ForCtorParam("DurationInMinutes", opt => opt.MapFrom(src => src.GroupSchedule!.TraineeGroup!.DurationInMinutes))
                .ForCtorParam("TotalEnrolled", opt => opt.MapFrom(src => src.GroupSchedule!.TraineeGroup!.Enrollments.Count(e => e.IsActive)))
                // Previously hardcoded to 0 - these never reflected actual marks, so the
                // session summary counters never moved no matter what a coach marked. Counting
                // straight off this session's own Attendances (not the roster/Enrollments) is
                // correct even for a partially-marked session: an unmarked trainee simply isn't
                // counted anywhere yet, matching the "who's been marked so far" meaning of these
                // counters (GetBySessionOccurrenceAsync's own roster still defaults an unmarked
                // trainee to Absent for the marking UI, which is a separate, UI-only default).
                .ForCtorParam("TotalPresent", opt => opt.MapFrom(src => src.Attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present)))
                .ForCtorParam("TotalLate", opt => opt.MapFrom(src => src.Attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Late)))
                .ForCtorParam("TotalAbsent", opt => opt.MapFrom(src => src.Attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Absent)))
                .ReverseMap();

            CreateMap<CreateSessionOccurrenceCommand, SessionOccurrence>();

            CreateMap<UpdateSessionOccurrenceCommand, SessionOccurrence>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
