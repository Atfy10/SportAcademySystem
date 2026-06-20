using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Queries.SportPriceQueries.GetById
{
    public class GetSportPriceByKeyQueryHandler : IRequestHandler<GetSportPriceByKeyQuery, Result<SportPriceDto>>
    {
        private readonly ISportPriceRepository _sportPriceRepository;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetSportPriceByKeyQueryHandler(ISportPriceRepository sportPriceRepository)
        {
            _sportPriceRepository = sportPriceRepository;
        }

        public async Task<Result<SportPriceDto>> Handle(GetSportPriceByKeyQuery request, CancellationToken cancellationToken)
        {
            var sportPrice = await _sportPriceRepository.GetByKeyWithIncludesAsync(request.BranchId,
                request.SportId, request.SubsTypeId, cancellationToken)
                ?? throw new SportPriceNotFoundException($"{request.BranchId}, {request.SportId}, {request.SubsTypeId}");

            var dto = sportPrice.ToDto();
            return Result<SportPriceDto>.Success(dto, _operationType);
        }
    }
}
