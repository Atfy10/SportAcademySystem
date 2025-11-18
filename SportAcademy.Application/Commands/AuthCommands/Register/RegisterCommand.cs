using MediatR;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.AuthCommands.Register
{
    public record RegisterCommand(
        string UserName,
        string Email,
        string Password,
        string PhoneNumber,
        bool EmailConfirmed = false
        ) : IRequest<Result<string>>;
}
