using SportAcademy.Application.Commands.SportCommands.CreateSport;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.SportProfile
{
    public class SportProfile : AutoMapper.Profile
    {
        public SportProfile()
        {
            CreateMap<Sport, SportDropDownListDto>()
                .ReverseMap();

            // Name/Description are NOT translated through this map. AutoMapper Profiles are
            // configured once at startup and ProjectTo compiles a cached expression tree from
            // them, so there is no way to splice a per-request language into it here - the
            // repository builds the translated projection itself instead (see SportRepository),
            // capturing the current language as a local variable that EF Core parameterizes
            // correctly. This mapping still backs the reverse Command -> Sport direction.
            CreateMap<Sport, SportDto>()
                .ReverseMap()
                .ForMember(dest => dest.Coaches, opt => opt.Ignore())
                .ForMember(dest => dest.SubscriptionTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Branches, opt => opt.Ignore())
                .ForMember(dest => dest.Trainees, opt => opt.Ignore())
                .ForMember(dest => dest.Translations, opt => opt.Ignore());

            CreateMap<CreateSportCommand, Sport>()
                .ForMember(dest => dest.Coaches, opt => opt.Ignore())
                .ForMember(dest => dest.SubscriptionTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Branches, opt => opt.Ignore())
                .ForMember(dest => dest.Trainees, opt => opt.Ignore())
                .ForMember(dest => dest.Translations, opt => opt.Ignore());

            // UpdateSportCommand -> Sport is no longer an AutoMapper mapping -
            // UpdateSportCommandHandler uses Mappings/Manual/SportMapper.cs instead.
        }
    }
}
