using SportAcademy.Application.Commands.AuthCommands.Register;
using SportAcademy.Application.Commands.UserCommands.UserCreate;
using SportAcademy.Application.Commands.UserCommands.UserUpdate;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class AppUserMappings
    {
        public static AppUser ToAppUser(this RegisterCommand cmd)
            => AppUser.Create(cmd.UserName, cmd.Email, cmd.PhoneNumber, cmd.EmailConfirmed);

        public static AppUser ToAppUser(this CreateUserCommand cmd)
            => AppUser.Create(cmd.UserName, cmd.Email, cmd.PhoneNumber, cmd.EmailConfirmed);

        public static void ApplyUpdate(this AppUser user, UpdateUserCommand cmd)
        {
            if (cmd.Email is not null)
                user.Email = cmd.Email;
            if (cmd.PhoneNumber is not null)
                user.PhoneNumber = cmd.PhoneNumber;
        }

        public static AppUserDto ToDto(this AppUser user)
            => new(user.Email!, user.UserName!, user.PhoneNumber);
    }
}
