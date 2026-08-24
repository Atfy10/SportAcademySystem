using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PaymentTypeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PaymentTypeCommands.UpdatePaymentType
{
    public class UpdatePaymentTypeCommandHandler : IRequestHandler<UpdatePaymentTypeCommand, Result<PaymentTypeDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IPaymentTypeRepository _repository;

        public UpdatePaymentTypeCommandHandler(IPaymentTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<PaymentTypeDto>> Handle(UpdatePaymentTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
                return Result<PaymentTypeDto>.Failure($"Payment type with ID {request.Id} not found.", _operation, 404);

            if (request.Name is not null)
                entity.Name = request.Name;

            if (request.IsActive.HasValue)
                entity.IsActive = request.IsActive.Value;

            // Setting this one as default clears whichever other type held it - exactly one
            // default per tenant. Explicitly un-defaulting the current default is a no-op here;
            // the tenant simply keeps its default until another type is flagged instead
            // (mirrors CreatePaymentTypeCommandHandler's "never leave zero defaults" guarantee).
            if (request.IsDefault is true && !entity.IsDefault)
            {
                await _repository.ClearDefaultFlagAsync(entity.Id, cancellationToken);
                entity.IsDefault = true;
            }

            await _repository.UpdateAsync(entity, cancellationToken);

            return Result<PaymentTypeDto>.Success(entity.ToDto(), _operation);
        }
    }
}
