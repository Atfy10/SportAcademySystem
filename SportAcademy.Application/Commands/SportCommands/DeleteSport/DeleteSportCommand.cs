using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.SportCommands.DeleteSport
{
    public record DeleteSportCommand(int Id) : IRequest<Result<bool>>;

}
