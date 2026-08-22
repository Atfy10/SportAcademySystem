using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.FinanceQueries.GetPayments;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, Result<PagedData<PaymentDto>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetPaymentsQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<PagedData<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _paymentRepository.GetPagedAsync(
            request.Page, request.BranchId, request.Method, request.Status, request.From, request.To, ct);

        var dtos = items.Select(p => new PaymentDto(
            p.PaymentNumber, p.Amount, p.RefundedAmount, p.Method, p.Status,
            p.PaidDate, p.Branch.Name, p.Currency, p.Reference, p.Notes
        )).ToList();

        return Result<PagedData<PaymentDto>>.Success(new PagedData<PaymentDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page.Page,
            PageSize = request.Page.PageSize,
        }, _operation);
    }
}
