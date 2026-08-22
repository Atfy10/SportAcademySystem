using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.FinanceQueries.GetInvoices;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<PagedData<InvoiceDto>>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<PagedData<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _invoiceRepository.GetPagedAsync(
            request.Page, request.BranchId, request.Status, ct);

        return Result<PagedData<InvoiceDto>>.Success(new PagedData<InvoiceDto>
        {
            Items = items.Select(InvoiceMapper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Page.Page,
            PageSize = request.Page.PageSize,
        }, _operation);
    }
}
