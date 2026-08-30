using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SportCommands.CreateSport
{
    public record CreateSportCommand(
        string Name,
        string? Description,
        SportCategory Category,
        bool IsRequireHealthTest,
        string? NameAr = null,
        string? DescriptionAr = null
    ) : IRequest<Result<int>>;
}
