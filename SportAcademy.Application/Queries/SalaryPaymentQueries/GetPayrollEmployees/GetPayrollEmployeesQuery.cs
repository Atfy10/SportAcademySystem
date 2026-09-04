using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Queries.SalaryPaymentQueries.GetPayrollEmployees;

public record GetPayrollEmployeesQuery(int? BranchId, string? Search) : IRequest<Result<List<PayrollEmployeeDto>>>;
