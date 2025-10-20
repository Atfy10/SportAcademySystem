using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.SportCommands.UpdateSport
{
	public record UpdateSportCommand(
		int Id,
		string? Name = null,
		string? Description = null,
		string? Category = null,
		bool? IsRequireHealthTest = null
	) : IRequest<Result<SportDto>>;
}
