using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SportCommands.CreateSport
{
    public record CreateSportCommand(
        string Name,
        string? Description,
        SportCategory Category,
        bool IsRequireHealthTest
    ) : IRequest<Result<int>>;
}
