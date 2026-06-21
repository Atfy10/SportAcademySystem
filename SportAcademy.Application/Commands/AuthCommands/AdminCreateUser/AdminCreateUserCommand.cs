using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos.AdminDtos;

namespace SportAcademy.Application.Commands.AuthCommands.AdminCreateUser;

public record AdminCreateUserCommand(
    string UserName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed = false,
    bool IsActive = true
) : IRequest<Result<AdminCreateUserResultDto>>;
