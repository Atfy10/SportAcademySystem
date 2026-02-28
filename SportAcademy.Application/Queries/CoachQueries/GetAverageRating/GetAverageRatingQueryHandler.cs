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
    public class GetAverageRatingQueryHandler : IRequestHandler<GetAverageRatingQuery, Result<int>>
    {
        private readonly ICoachRepository _coachRepository;
        public GetAverageRatingQueryHandler(ICoachRepository coachRepository)
        {
            _coachRepository = coachRepository;
        }
        public async Task<Result<int>> Handle(GetAverageRatingQuery request, CancellationToken cancellationToken)
        {
            var allCoachs = await _coachRepository.GetAllAsync(cancellationToken);

            if (!allCoachs.Any())
                return Result<int>.Success(0, "Get Average Rating");

            var average = allCoachs.Average(x => x.Rate);

            return Result<int>.Success((int)Math.Round(average), "Get Average Rating");
        }
    }
}
