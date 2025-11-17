using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateJwtToken(AppUser appUser, params string[] roles);
    }
}
