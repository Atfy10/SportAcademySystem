using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SportQueries.GetAll
{
    public class GetAllSportsQueryHandler : IRequestHandler<GetAllSportsQuery, Result<List<Sport>>>
    {
        private readonly ISportRepository _sportRepository;

        public GetAllSportsQueryHandler(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async Task<Result<List<Sport>>> Handle(GetAllSportsQuery request, CancellationToken cancellationToken)
        {
            var sports = await _sportRepository.GetAllAsync(cancellationToken);

            return Result<List<Sport>>.Success(sports, "All sports retrieved successfully.");
        }
    }
}
