using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.FinanceQueries.GetInvoices;

public record GetInvoicesQuery(PageRequest Page, int? BranchId, string? Status)
    : IRequest<Result<PagedData<InvoiceDto>>>;
