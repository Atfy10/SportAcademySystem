using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.SportDtos
{
    public record SportDto(
        int Id,
        string Name,
        string? Description,
        SportCategory Category,
        bool IsRequireHealthTest
    );

}
