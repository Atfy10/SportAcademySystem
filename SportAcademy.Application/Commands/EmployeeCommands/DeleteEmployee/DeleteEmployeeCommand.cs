using MediatR;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee
{
    public record DeleteEmployeeCommand(int Id) : IRequest<Result<bool>>;

}
