using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PaymentTypeQueries.GetById
{
    public class GetPaymentTypeByIdQueryHandler : IRequestHandler<GetPaymentTypeByIdQuery, Result<PaymentTypeDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly IPaymentTypeRepository _repository;
        private readonly ICurrentLanguageProvider _language;

        public GetPaymentTypeByIdQueryHandler(IPaymentTypeRepository repository, ICurrentLanguageProvider language)
        {
            _repository = repository;
            _language = language;
        }

        public async Task<Result<PaymentTypeDto>> Handle(GetPaymentTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdWithTranslationAsync(request.Id, cancellationToken);

            if (entity is null)
                return Result<PaymentTypeDto>.Failure($"Payment type with ID {request.Id} not found.", _operation, 404);

            return Result<PaymentTypeDto>.Success(entity.ToDto(_language.Language), _operation);
        }
    }
}
