using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.CoachQueries.GetCoachsCount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SportQueries.GetSportsCount
{
    public class GetSportsCountQueryHandler : IRequestHandler<GetSportsCountQuery, Result<int>>
    {
        private readonly ISportRepository _sportRepository;
        public GetSportsCountQueryHandler(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }
        public async Task<Result<int>> Handle(GetSportsCountQuery request, CancellationToken cancellationToken)
        {
            var allSports = await _sportRepository.GetAllAsync(cancellationToken);
            if (!allSports.Any())
                return Result<int>.Success(0, "Get Sports Count");
            return Result<int>.Success(allSports.Count(), "Get Sports Count");
        }
    }
}
