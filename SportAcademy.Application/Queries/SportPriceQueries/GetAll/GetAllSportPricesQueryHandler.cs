using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SportPriceQueries.GetAll
{
    public class GetAllSportPricesQueryHandler : IRequestHandler<GetAllSportPricesQuery, Result<List<SportPriceDto>>>
    {
        private readonly ISportPriceRepository _sportPriceRepository;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllSportPricesQueryHandler(ISportPriceRepository sportPriceRepository)
        {
            _sportPriceRepository = sportPriceRepository;
        }

        public async Task<Result<List<SportPriceDto>>> Handle(GetAllSportPricesQuery request, CancellationToken cancellationToken)
        {
            var prices = await _sportPriceRepository.GetAllWithIncludesAsync(cancellationToken) 
                ?? [];

            var dtoList = prices.Select(p => p.ToDto()).ToList();

            return Result<List<SportPriceDto>>.Success(dtoList, _operationType);
        }
    }
}
