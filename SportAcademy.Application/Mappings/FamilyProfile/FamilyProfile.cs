using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.FamilyProfile
{
    public class FamilyProfile : AutoMapper.Profile
    {
        public FamilyProfile()
        {
            CreateMap<Family, FamilyDto>()
                .ConstructUsing(src => new FamilyDto(src.Id, src.FamilyCode))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.FamilyCode))
                .ReverseMap();
        }
    }
}
