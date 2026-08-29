using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PaymentTypeCommands.CreatePaymentType
{
    public class CreatePaymentTypeCommandHandler : IRequestHandler<CreatePaymentTypeCommand, Result<PaymentTypeDto>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly IPaymentTypeRepository _repository;
        private readonly ICurrentLanguageProvider _language;

        public CreatePaymentTypeCommandHandler(IPaymentTypeRepository repository, ICurrentLanguageProvider language)
        {
            _repository = repository;
            _language = language;
        }

        public async Task<Result<PaymentTypeDto>> Handle(CreatePaymentTypeCommand request, CancellationToken cancellationToken)
        {
            // The tenant's very first payment type must be the default - otherwise "mark
            // enrollment as paid" would have no candidate to record against until an admin
            // remembers to flag one, which would silently 409 that everyday action.
            var isFirstPaymentType = !await _repository.AnyAsync(cancellationToken);
            var isDefault = isFirstPaymentType || request.IsDefault;

            if (isDefault && !isFirstPaymentType)
                await _repository.ClearDefaultFlagAsync(null, cancellationToken);

            var entity = new PaymentType
            {
                Name = request.Name,
                IsActive = request.IsActive,
                IsDefault = isDefault,
            };

            await _repository.AddAsync(entity, cancellationToken);

            return Result<PaymentTypeDto>.Success(entity.ToDto(_language.Language), _operation);
        }
    }
}
