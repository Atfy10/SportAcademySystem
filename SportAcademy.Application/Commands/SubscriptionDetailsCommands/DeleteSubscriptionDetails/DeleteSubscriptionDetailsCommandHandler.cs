using AutoMapper;
using MediatR;
using SportAcademy.Application.Common;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.PaymentExceptions;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;
using SportAcademy.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.DeleteSubscriptionDetails
{
    public class DeleteSubscriptionDetailsCommandHandler : IRequestHandler<DeleteSubscriptionDetailsCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IMapper _mapper;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService managementService,
            IMapper mapper,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher,
            IInvoiceRepository invoiceRepository,
            IUnitOfWork unitOfWork
            )
        {
            _mapper = mapper;
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = managementService;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(DeleteSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await  _subscriptionDetailsRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            // Deleting a subscription must not leave its bill behind. Money actually held
            // against it has to be given back (refund/void from the receipt) first, so the
            // payment records and reports stay true; an unpaid bill is simply cancelled so it
            // stops showing as owed / overdue.
            var invoice = await _invoiceRepository.GetBySubscriptionDetailsIdAsync(subDetails.Id, cancellationToken);
            if (invoice is not null && invoice.AmountPaid > 0)
                throw FinanceRuleException.SubscriptionHasPayments();

            await _unitOfWork.InTransactionAsync(async () =>
            {
                if (invoice is not null && invoice.Status != InvoiceStatus.Cancelled)
                {
                    invoice.Status = InvoiceStatus.Cancelled;
                    await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
                }

                await _subscriptionDetailsRepository.DeleteAsync(subDetails, cancellationToken);
                return true;
            }, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var actorName = _userContext.UserId is { } userId
                ? await _userRepository.GetDisplayNameAsync(userId, cancellationToken)
                : "System";
            await _publisher.Publish(new SubscriptionLifecycleEvent(subDetails.Id, "Deleted", actorName), cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
