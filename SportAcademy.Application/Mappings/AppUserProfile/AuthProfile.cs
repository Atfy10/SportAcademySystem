using SportAcademy.Application.Commands.AuthCommands.Register;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
