using MediatR;
using SportAcademy.Application.Common.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetTraineesCountOfSpecificDay
{
    public record GetTraineesCountOfSpecificDayQuery(DateTime Date) : IRequest<Result<int>>;
}
