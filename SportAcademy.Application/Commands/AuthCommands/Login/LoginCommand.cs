using MediatR;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.AuthCommands.Login
{
    public record LoginCommand(
        string UserNameOrEmail,
        string Password
        ) : IRequest<Result<string>>;
}
