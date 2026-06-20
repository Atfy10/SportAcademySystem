using AutoMapper;
using SportAcademy.Application.DTOs.NationalityCategoryDtos;
using SportAcademy.Domain.Entities;
using AutoMapperProfile = AutoMapper.Profile;

namespace SportAcademy.Infrastructure.Mappings
{
    public class NationalityCategoryMappingProfile : AutoMapperProfile
    {
        public NationalityCategoryMappingProfile()
        {
            CreateMap<NationalityCategory, NationalityCategoryDto>()
                .ConstructUsing(src => new NationalityCategoryDto
                {
                    Id = src.Id,
                    Code = src.Code,
                    Name = src.Name
                });
        }
    }
}
