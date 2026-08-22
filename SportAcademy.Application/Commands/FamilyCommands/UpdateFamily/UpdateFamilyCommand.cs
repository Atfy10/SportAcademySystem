using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;

namespace SportAcademy.Application.Commands.FamilyCommands.UpdateFamily;

public record UpdateFamilyCommand(
    int Id,
    string? Name,
    string? GuardianName,
    string? GuardianPhone
) : IRequest<Result<FamilyDto>>;
