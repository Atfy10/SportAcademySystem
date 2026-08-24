using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.PaymentTypeExceptions;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdatePaymentStatus;

public class UpdatePaymentStatusCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IInvoiceRepository invoiceRepository,
    IFinanceLedgerService financeLedgerService,
    IPaymentTypeRepository paymentTypeRepository,
    IUserContextService userContext)
    : IRequestHandler<UpdatePaymentStatusCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdatePaymentStatusCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await ((IBaseRepository<Enrollment, int>)enrollmentRepository)
            .GetByIdAsync(request.Id, cancellationToken)
            ?? throw new IdNotFoundException("Enrollment", request.Id.ToString());

        if (request.PaymentStatus == "Paid")
        {
            var invoice = await invoiceRepository.GetBySubscriptionDetailsIdAsync(
                enrollment.SubscriptionDetailsId, cancellationToken)
                ?? throw new IdNotFoundException("Invoice", $"subscription {enrollment.SubscriptionDetailsId}");

            var outstanding = invoice.GrandTotal - invoice.AmountPaid;
            if (outstanding > 0)
            {
                // The tenant's flagged-default payment type stands in for the old hardcoded
                // PaymentMethod.Cash - falls back to the first active type if none is flagged
                // default (shouldn't normally happen: PaymentType creation always keeps exactly
                // one default), and only 409s if the tenant has configured none at all yet.
                var defaultPaymentType = await paymentTypeRepository.GetDefaultAsync(cancellationToken)
                    ?? await paymentTypeRepository.GetFirstActiveAsync(cancellationToken)
                    ?? throw new NoDefaultPaymentTypeException();

                await financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
                    Amount: outstanding,
                    PaymentTypeId: defaultPaymentType.Id,
                    BranchId: invoice.BranchId,
                    Currency: invoice.Currency,
                    Reference: null,
                    Notes: "Marked paid via enrollment payment-status update.",
                    RecordedByUserId: userContext.UserId,
                    Allocations: [new PaymentAllocationInput(invoice.Id, outstanding)]
                ), cancellationToken);
            }
        }

        return Result<bool>.Success(true, OperationType.Update.ToString());
    }
}
