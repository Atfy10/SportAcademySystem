using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.SportTraineeCommands.DeleteSportTrainee
{
	public record DeleteSportTraineeCommand(
		int SportId,
		int TraineeId
	) : IRequest<Result<string>>;
}
