using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PaymentTypeQueries.GetAll
{
    public class GetAllPaymentTypesQueryHandler : IRequestHandler<GetAllPaymentTypesQuery, Result<List<PaymentTypeDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly IPaymentTypeRepository _repository;
        private readonly ICurrentLanguageProvider _language;

        public GetAllPaymentTypesQueryHandler(IPaymentTypeRepository repository, ICurrentLanguageProvider language)
        {
            _repository = repository;
            _language = language;
        }

        public async Task<Result<List<PaymentTypeDto>>> Handle(GetAllPaymentTypesQuery request, CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync(cancellationToken);

            var dtos = entities.Select(e => e.ToDto(_language.Language)).ToList();

            return Result<List<PaymentTypeDto>>.Success(dtos, _operation);
        }
    }
}
