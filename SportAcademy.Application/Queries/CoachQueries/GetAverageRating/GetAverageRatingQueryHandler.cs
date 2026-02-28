using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.CoachQueries.GetAverageRating
{
    public class GetAverageRatingQueryHandler : IRequestHandler<GetAverageRatingQuery, Result<double>>
    {
        private readonly ICoachRepository _coachRepository;
        public GetAverageRatingQueryHandler(ICoachRepository coachRepository)
        {
            _coachRepository = coachRepository;
        }
        public async Task<Result<double>> Handle(GetAverageRatingQuery request, CancellationToken cancellationToken)
        {
            var average = await _coachRepository.GetAverageRatingAsync(cancellationToken);

            var result = average.HasValue
                ? Math.Round(average.Value, 1)
                : 0;

            return Result<double>.Success(result, "Get Average Rating");
        }
    }
}
