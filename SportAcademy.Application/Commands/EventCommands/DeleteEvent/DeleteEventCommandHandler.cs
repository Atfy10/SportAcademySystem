using MediatR;
using SportAcademy.Application.Common;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;

namespace SportAcademy.Application.Commands.EventCommands.DeleteEvent
{
    // For a booking entered by mistake: only allowed while no payment has ever touched its
    // invoice. Anything with money history is cancelled instead, so the finance trail stays whole.
    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly IEventRepository _eventRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteEventCommandHandler(
            IEventRepository eventRepository,
            IInvoiceRepository invoiceRepository,
            IUnitOfWork unitOfWork)
        {
            _eventRepository = eventRepository;
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var ev = await _eventRepository.GetWithDetailsAsync(request.Id, forUpdate: true, cancellationToken)
                ?? throw new EventNotFoundException(request.Id.ToString());

            if (ev.Invoice is { } invoice && invoice.Allocations.Count > 0)
                throw EventRuleException.HasPayments();

            await _unitOfWork.InTransactionAsync(async () =>
            {
                if (ev.Invoice is { } unpaid)
                {
                    unpaid.Status = InvoiceStatus.Cancelled;
                    await _invoiceRepository.DeleteAsync(unpaid, cancellationToken);
                }

                await _eventRepository.DeleteAsync(ev, cancellationToken);
                return true;
            }, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
