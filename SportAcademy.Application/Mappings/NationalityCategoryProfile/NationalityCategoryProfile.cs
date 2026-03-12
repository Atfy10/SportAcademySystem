using SportAcademy.Application.DTOs.NationalityCategoryDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.NationalityCategoryProfile
{
    public class NationalityCategoryProfile : AutoMapper.Profile
    {
        public NationalityCategoryProfile()
        {
            CreateMap<NationalityCategory, NationalityCategoryDto>();
        }
    }
}
