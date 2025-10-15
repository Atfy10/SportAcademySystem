using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee
{
	public record CreateSportTraineeCommand(
		int SportId,
		int TraineeId,
		string SkillLevel
	) : IRequest<Result<string>>;

}
