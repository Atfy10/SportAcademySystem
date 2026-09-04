using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DiscountCodeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.DiscountCodeQueries.GetAll
{
    public class GetAllDiscountCodesQueryHandler : IRequestHandler<GetAllDiscountCodesQuery, Result<List<DiscountCodeDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly IDiscountCodeRepository _repository;

        public GetAllDiscountCodesQueryHandler(IDiscountCodeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<DiscountCodeDto>>> Handle(GetAllDiscountCodesQuery request, CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync(cancellationToken);

            var dtos = entities
                .Select(c => new DiscountCodeDto(c.Id, c.Code, c.Description, c.PercentageOff, c.IsActive, c.ExpiresAt))
                .ToList();

            return Result<List<DiscountCodeDto>>.Success(dtos, _operation);
        }
    }
}
