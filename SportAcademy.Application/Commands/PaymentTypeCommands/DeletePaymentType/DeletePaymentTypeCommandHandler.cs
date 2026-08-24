using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.PaymentTypeExceptions;

namespace SportAcademy.Application.Commands.PaymentTypeCommands.DeletePaymentType
{
    public class DeletePaymentTypeCommandHandler : IRequestHandler<DeletePaymentTypeCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly IPaymentTypeRepository _repository;

        public DeletePaymentTypeCommandHandler(IPaymentTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeletePaymentTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
                return Result<bool>.Failure($"Payment type with ID {request.Id} not found.", _operation, 404);

            if (await _repository.HasPaymentsAsync(entity.Id, cancellationToken))
                throw new PaymentTypeInUseException(entity.Id);

            await _repository.DeleteAsync(entity, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
