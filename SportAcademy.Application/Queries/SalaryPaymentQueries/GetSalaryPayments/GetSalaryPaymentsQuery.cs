using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.SalaryPaymentQueries.GetSalaryPayments;

public record GetSalaryPaymentsQuery(
    PageRequest Page, int? EmployeeId, int? BranchId, string? Status, DateOnly? PeriodFrom, DateOnly? PeriodTo
) : IRequest<Result<PagedData<SalaryPaymentDto>>>;
