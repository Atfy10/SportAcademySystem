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
            var count = await _sportRepository.CountAsync(cancellationToken);
            return Result<int>.Success(count, "Get Sports Count");
        }
    }
}
