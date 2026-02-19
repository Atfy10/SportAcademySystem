using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SportCommands.UpdateSport
{
    public record UpdateSportCommand(
        int Id,
        string Name,
        string? Description,
        SportCategory Category,
        bool IsRequireHealthTest
    ) : IRequest<Result<SportDto>>;
}
