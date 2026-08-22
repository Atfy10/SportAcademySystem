using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.FinanceQueries.GetOutstandingBalances;

public record GetOutstandingBalancesQuery(PageRequest Page, int? BranchId, bool OverdueOnly)
    : IRequest<Result<PagedData<OutstandingInvoiceDto>>>;
