using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.FamilyProfile
{
    public class FamilyProfile : AutoMapper.Profile
    {
        public FamilyProfile()
        {
            // .ForCtorParam() here, not .ConstructUsing() - ConstructUsing opts a mapping out
            // of AutoMapper's LINQ expression-tree translation, which ProjectTo relies on to
            // turn this into a SQL projection (same root cause as the Coach dropdown fix). Plain
            // .ForMember() doesn't work either: FamilyDto is a positional record with no
            // parameterless constructor, so AutoMapper must be told how to fill constructor
            // parameters explicitly via ForCtorParam.
            CreateMap<Family, FamilyDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("Code", opt => opt.MapFrom(src => src.FamilyCode))
                .ReverseMap();
        }
    }
}
