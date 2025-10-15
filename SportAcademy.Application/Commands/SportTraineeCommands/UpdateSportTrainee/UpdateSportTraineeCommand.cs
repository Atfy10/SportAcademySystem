using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee
{
	public record UpdateSportTraineeCommand(
		int SportId,
		int TraineeId,
		string SkillLevel
	) : IRequest<Result<string>>;
}
