using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.FinanceQueries.GetPayments;

public record GetPaymentsQuery(
    PageRequest Page, int? BranchId, int? PaymentTypeId, string? Status, DateTime? From, DateTime? To,
    string? Term = null
) : IRequest<Result<PagedData<PaymentDto>>>;
