using AutoMapper;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Mappings
{
    public class SessionOccurrenceMappingProfile : AutoMapper.Profile
    {
        public SessionOccurrenceMappingProfile()
        {
            CreateMap<SessionOccurrence, SessionOccurrenceDto>()
                .ConstructUsing(src => new SessionOccurrenceDto(
                    src.Id,
                    src.GroupSchedule!.TraineeGroup.Id,
                    DateOnly.FromDateTime(src.StartDateTime),
                    src.GroupSchedule!.TraineeGroup!.Coach!.Sport!.Name,
                    $"{src.GroupSchedule!.TraineeGroup!.Coach.Employee!.FirstName} {src.GroupSchedule!.TraineeGroup!.Coach.Employee.LastName}",
                    src.GroupSchedule!.TraineeGroup!.Branch!.Name,
                    src.StartDateTime.ToString("HH:mm:ss"),
                    src.GroupSchedule!.TraineeGroup!.DurationInMinutes,
                    src.GroupSchedule!.TraineeGroup!.Enrollments.Count(e => e.IsActive),
                    0,
                    0,
                    0
                ));
        }
    }
}
