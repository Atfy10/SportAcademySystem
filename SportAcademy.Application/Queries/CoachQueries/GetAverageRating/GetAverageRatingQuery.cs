using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.CoachQueries.GetAverageRating
{
    public record GetAverageRatingQuery : IRequest<Result<double>>
    {
    }
}
