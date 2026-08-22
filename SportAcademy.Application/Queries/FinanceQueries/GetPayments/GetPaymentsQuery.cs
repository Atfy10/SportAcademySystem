using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.FinanceQueries.GetPayments;

public record GetPaymentsQuery(
    PageRequest Page, int? BranchId, string? Method, string? Status, DateTime? From, DateTime? To
) : IRequest<Result<PagedData<PaymentDto>>>;
