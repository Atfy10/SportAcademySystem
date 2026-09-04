using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SalaryPaymentExceptions;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.DeleteSalaryPayment
{
    public class DeleteSalaryPaymentCommandHandler : IRequestHandler<DeleteSalaryPaymentCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly ISalaryPaymentRepository _repository;

        public DeleteSalaryPaymentCommandHandler(ISalaryPaymentRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteSalaryPaymentCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SalaryPaymentNotFoundException(request.Id.ToString());

            if (entity.Status != SalaryPaymentStatus.PendingApproval)
                throw new InvalidSalaryPaymentTransitionException(entity.Id, entity.Status.ToString(), "deleted");

            await _repository.DeleteAsync(entity, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
