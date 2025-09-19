using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Mappings.AppUserProfile
{
    public class AppUserProfile : AutoMapper.Profile
    {
        public AppUserProfile()
        {
            CreateMap<AppUser, AppUserDto>()
                .ConstructUsing(src => new AppUserDto(
                    src.Email,
                    src.UserName,
                    src.PhoneNumber
                ))
                .ReverseMap();
        }
    }
}
