using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.EnrollmentCommands.UpdatePaymentStatus;

public class UpdatePaymentStatusCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IInvoiceRepository invoiceRepository,
    IFinanceLedgerService financeLedgerService,
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
                await financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
                    Amount: outstanding,
                    Method: PaymentMethod.Cash,
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
