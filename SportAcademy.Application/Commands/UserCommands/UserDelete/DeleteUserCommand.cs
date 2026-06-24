using MediatR;
using SportAcademy.Application.Common.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.UserCommands.UserDelete
{
    public record DeleteUserCommand(Guid Id) : IRequest<Result<bool>>;
}
