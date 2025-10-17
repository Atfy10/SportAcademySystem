using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Queries.SportTraineeQueries.GetById
{
	public record GetSportTraineeByKeyQuery(
	   int SportId,
	   int TraineeId
   ) : IRequest<Result<SportTraineeDto>>;
}
