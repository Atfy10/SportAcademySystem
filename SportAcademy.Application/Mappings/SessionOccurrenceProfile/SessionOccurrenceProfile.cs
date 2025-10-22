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
            CreateMap<SessionOccurrence, SessionOccurrenceDto>()
                .ReverseMap();

            CreateMap<CreateSessionOccurrenceCommand, SessionOccurrence>();

            CreateMap<UpdateSessionOccurrenceCommand, SessionOccurrence>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
