using AutoMapper;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Domain.Entities;
using AutoMapperProfile = AutoMapper.Profile;

namespace SportAcademy.Infrastructure.Mappings
{
    public class AppUserMappingProfile : AutoMapperProfile
    {
        public AppUserMappingProfile()
        {
            CreateMap<AppUser, AppUserDto>()
                .ConstructUsing(src => new AppUserDto(
                    src.Email!,
                    src.UserName!,
                    src.PhoneNumber!
                ));

            CreateMap<AppUser, AppUserCardDto>()
                .ConstructUsing(src => new AppUserCardDto
                {
                    Id = src.Id,
                    UserName = src.UserName!,
                    Email = src.Email!,
                    IsActive = !src.IsBanned
                });
        }
    }
}
