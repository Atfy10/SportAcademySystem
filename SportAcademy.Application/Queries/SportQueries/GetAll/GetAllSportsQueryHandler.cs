using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SportQueries.GetAll
{
    public class GetAllSportsQueryHandler : IRequestHandler<GetAllSportsQuery, Result<IReadOnlyList<SportDto>>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllSportsQueryHandler(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async Task<Result<IReadOnlyList<SportDto>>> Handle(GetAllSportsQuery request, CancellationToken cancellationToken)
        {
            var sports = await _sportRepository.GetAllAsync(cancellationToken);

            var sportsDto = sports.Select(s => s.ToDto()).ToList();

            return Result<IReadOnlyList<SportDto>>.Success(sportsDto, _operationType);
        }
    }
}
