using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportTraineeDtos;

namespace SportAcademy.Application.Queries.SportTraineeQueries.GetAll
{
	public record GetAllSportTraineesQuery() : IRequest<Result<List<SportTraineeDto>>>;

}
