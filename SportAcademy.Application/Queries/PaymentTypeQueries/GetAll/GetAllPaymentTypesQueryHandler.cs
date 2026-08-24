using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PaymentTypeQueries.GetAll
{
    public class GetAllPaymentTypesQueryHandler : IRequestHandler<GetAllPaymentTypesQuery, Result<List<PaymentTypeDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly IPaymentTypeRepository _repository;

        public GetAllPaymentTypesQueryHandler(IPaymentTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<PaymentTypeDto>>> Handle(GetAllPaymentTypesQuery request, CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync(cancellationToken);

            var dtos = entities.Select(e => e.ToDto()).ToList();

            return Result<List<PaymentTypeDto>>.Success(dtos, _operation);
        }
    }
}
