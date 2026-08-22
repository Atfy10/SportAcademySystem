using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.FinanceQueries.GetOutstandingBalances;

public class GetOutstandingBalancesQueryHandler : IRequestHandler<GetOutstandingBalancesQuery, Result<PagedData<OutstandingInvoiceDto>>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetOutstandingBalancesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<PagedData<OutstandingInvoiceDto>>> Handle(GetOutstandingBalancesQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _invoiceRepository.GetOutstandingAsync(
            request.Page, request.BranchId, request.OverdueOnly, ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dtos = items.Select(i => new OutstandingInvoiceDto(
            i.Id, i.InvoiceNumber,
            i.Trainee is null ? null : $"{i.Trainee.FirstName} {i.Trainee.LastName}",
            i.BranchId, i.Branch.Name, i.DueDate, i.GrandTotal, i.AmountPaid, i.GrandTotal - i.AmountPaid,
            i.DueDate < today
        )).ToList();

        return Result<PagedData<OutstandingInvoiceDto>>.Success(new PagedData<OutstandingInvoiceDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page.Page,
            PageSize = request.Page.PageSize,
        }, _operation);
    }
}
