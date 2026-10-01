using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;

namespace SportAcademy.Application.Commands.EventCustomerCommands.DeleteEventCustomer
{
    // Only a customer with no events on record can be deleted (e.g. one added by mistake) - one
    // with history is deactivated instead, so their events and invoices keep pointing at them.
    public class DeleteEventCustomerCommandHandler : IRequestHandler<DeleteEventCustomerCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly IEventCustomerRepository _repository;

        public DeleteEventCustomerCommandHandler(IEventCustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteEventCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EventCustomerNotFoundException(request.Id.ToString());

            if (await _repository.HasEventsAsync(customer.Id, cancellationToken))
                throw EventRuleException.CustomerHasEvents();

            await _repository.DeleteAsync(customer, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
