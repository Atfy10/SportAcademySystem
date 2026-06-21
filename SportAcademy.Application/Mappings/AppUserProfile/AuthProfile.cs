using SportAcademy.Application.Commands.AuthCommands.AdminCreateUser;
using SportAcademy.Application.Commands.AuthCommands.Register;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.AppUserProfile
{
    public class AuthProfile : AutoMapper.Profile
    {
        public AuthProfile()
        {
            ShouldMapProperty = p => 
                p.Name != nameof(AppUser.PasswordHash)
                && p.Name != nameof(AppUser.SecurityStamp);

            CreateMap<RegisterCommand, AppUser>()
                .ForAllMembers(opt =>
                {
                    if (opt.DestinationMember.Name != nameof(AppUser.UserName) &&
                        opt.DestinationMember.Name != nameof(AppUser.Email) &&
                        opt.DestinationMember.Name != nameof(AppUser.PhoneNumber) &&
                        opt.DestinationMember.Name != nameof(AppUser.EmailConfirmed))
                    {
                        opt.Ignore();
                    }
                });

            CreateMap<AdminCreateUserCommand, AppUser>()
                .ForAllMembers(opt =>
                {
                    if (opt.DestinationMember.Name != nameof(AppUser.UserName) &&
                        opt.DestinationMember.Name != nameof(AppUser.Email) &&
                        opt.DestinationMember.Name != nameof(AppUser.PhoneNumber) &&
                        opt.DestinationMember.Name != nameof(AppUser.EmailConfirmed))
                    {
                        opt.Ignore();
                    }
                });
        }
    }
}
