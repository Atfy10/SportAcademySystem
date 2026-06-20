using AutoMapper;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Mappings
{
    public class SportMappingProfile : AutoMapper.Profile
    {
        public SportMappingProfile()
        {
            CreateMap<Sport, SportDto>()
                .ConstructUsing(src => new SportDto(
                    src.Id,
                    src.Name,
                    src.Description,
                    src.Category,
                    src.IsRequireHealthTest
                ));

            CreateMap<Sport, SportDropDownListDto>()
                .ConstructUsing(src => new SportDropDownListDto(
                    src.Id,
                    src.Name
                ));
        }
    }
}
