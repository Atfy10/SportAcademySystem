using MediatR;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.TraineeGroupCommands.DeleteTraineeGroup
{
    public record DeleteTraineeGroupCommand(int Id) : IRequest<Result<bool>>;
}
