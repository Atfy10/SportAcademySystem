using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Mappings.SportProfile
{
    public class SportProfile : AutoMapper.Profile
    {
        public SportProfile()
        {
            CreateMap<Sport, SportDto>().ReverseMap();

            //CreateMap<Domain.Entities.Sport, DTOs.SportDtos.CreateSportDto>().ReverseMap();

            //CreateMap<Domain.Entities.Sport, DTOs.SportDtos.UpdateSportDto>().ReverseMap();
        }
    }
}
