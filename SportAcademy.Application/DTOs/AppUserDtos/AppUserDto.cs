using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.AppUserDtos
{
    public record AppUserDto(
        string Email,
        string UserName,
        string PhoneNumber
    );
}
