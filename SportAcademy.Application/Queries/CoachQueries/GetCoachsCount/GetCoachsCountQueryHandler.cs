using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.CoachQueries.GetAverageRating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.CoachQueries.GetCoachsCount
{
    public class GetCoachsCountQueryHandler : IRequestHandler<GetCoachsCountQuery, Result<int>>
    {
        private readonly ICoachRepository _coachRepository;
        public GetCoachsCountQueryHandler(ICoachRepository coachRepository)
        {
            _coachRepository = coachRepository;
        }
        public async Task<Result<int>> Handle(GetCoachsCountQuery request, CancellationToken cancellationToken)
        {
            var allCoachs = await _coachRepository.GetAllAsync(cancellationToken);
            if (!allCoachs.Any())
                return Result<int>.Success(0, "Get Coachs Count");
            return Result<int>.Success(allCoachs.Count(), "Get Coachs Count");
        }
    }
}
