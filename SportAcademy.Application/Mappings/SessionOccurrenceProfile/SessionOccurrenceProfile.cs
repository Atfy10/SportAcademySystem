using SportAcademy.Application.Commands.SessionOccurrenceCommands.CreateSessionOccurrence;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.UpdateSessionOccurrence;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.SessionOccurrenceProfile
{
    public class SessionOccurrenceMappingProfile : AutoMapper.Profile
    {
        public SessionOccurrenceMappingProfile()
        {
            CreateMap<SessionOccurrence, SessionOccurrenceCardDto>()
    .ConstructUsing(src => new SessionOccurrenceCardDto(
        src.GroupSchedule.TraineeGroup.Name,
        src.GroupSchedule.TraineeGroup.Coach.Sport.Name,
        src.Status,
        src.StartDateTime.Date,
        src.StartDateTime.TimeOfDay,
        src.StartDateTime
            .AddMinutes(src.GroupSchedule.TraineeGroup.DurationInMinutes)
            .TimeOfDay,
        src.GroupSchedule.TraineeGroup.Coach.Employee.FirstName
            + " " +
        src.GroupSchedule.TraineeGroup.Coach.Employee.LastName,
        src.GroupSchedule.TraineeGroup.Branch.Name
    ))
    .ReverseMap();
            CreateMap<SessionOccurrence, SessionOccurrenceDto>()
                .ReverseMap();

            CreateMap<CreateSessionOccurrenceCommand, SessionOccurrence>();

            CreateMap<UpdateSessionOccurrenceCommand, SessionOccurrence>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
