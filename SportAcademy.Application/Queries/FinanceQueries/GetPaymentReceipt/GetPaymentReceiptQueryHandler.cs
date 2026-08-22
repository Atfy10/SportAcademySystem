using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.FinanceQueries.GetPaymentReceipt;

public class GetPaymentReceiptQueryHandler : IRequestHandler<GetPaymentReceiptQuery, Result<PaymentReceiptDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetPaymentReceiptQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<PaymentReceiptDto>> Handle(GetPaymentReceiptQuery request, CancellationToken ct)
    {
        var payment = await _paymentRepository.GetWithAllocationsAsync(request.PaymentNumber, ct)
            ?? throw new IdNotFoundException("Payment", request.PaymentNumber);

        var dto = new PaymentReceiptDto(
            payment.PaymentNumber,
            payment.Amount,
            payment.RefundedAmount,
            payment.Method,
            payment.Status,
            payment.PaidDate,
            payment.Branch.Name,
            payment.Currency,
            payment.Reference,
            payment.Notes,
            payment.Allocations.Select(a => new PaymentReceiptAllocationDto(
                a.InvoiceId, a.Invoice.InvoiceNumber, a.Amount)).ToList()
        );

        return Result<PaymentReceiptDto>.Success(dto, _operation);
    }
}
