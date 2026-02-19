using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.UserCommands.UserUpdate
{
    public record UpdateUserCommand(
        string Username,
        string? Email,
        string? PhoneNumber) : IRequest<Result<AppUserDto>>;
}
