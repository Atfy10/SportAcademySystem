using SportAcademy.Application.Commands.UserCommands.UserCreate;
using SportAcademy.Application.Commands.UserCommands.UserUpdate;
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
            ShouldMapProperty = p => p.Name != nameof(AppUser.PasswordHash)
                && p.Name != nameof(AppUser.SecurityStamp);

            CreateMap<AppUser, AppUserDto>()
                .ConstructUsing(src => new AppUserDto(
                    src.Email!,
                    src.UserName!,
                    src.PhoneNumber!
                ))
                .ReverseMap();

            CreateMap<AppUser, CreateUserCommand>()
                .ConstructUsing(src => new CreateUserCommand(
                    src.UserName!,
                    src.Email!,
                    src.PhoneNumber!,
                    src.EmailConfirmed
                ))
                .ReverseMap();

            CreateMap<AppUser, UpdateUserCommand>()
                .ConstructUsing(src => new UpdateUserCommand(
                    src.Email!,
                    src.UserName!,
                    src.PhoneNumber!
                ))
                .ReverseMap()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
