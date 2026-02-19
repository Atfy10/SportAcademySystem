using MediatR;
using SportAcademy.Application.Common.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.UserCommands.UserCreate
{
    public record CreateUserCommand(
        string UserName,
        string Email,
        string PhoneNumber,
        bool EmailConfirmed = false) : IRequest<Result<string>>;
}
