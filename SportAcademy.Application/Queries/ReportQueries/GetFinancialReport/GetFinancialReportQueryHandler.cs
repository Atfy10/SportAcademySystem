using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ReportQueries.GetFinancialReport;

// Merges three independently-grouped monthly series (revenue, expenses, paid salaries) on
// their shared "yyyy-MM" GroupKey - same no-default-window behavior as
// GetRevenueReportQueryHandler: From/To null simply means "don't filter that bound", not "use
// some computed default range".
public class GetFinancialReportQueryHandler : IRequestHandler<GetFinancialReportQuery, Result<List<FinancialReportRow>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ISalaryPaymentRepository _salaryPaymentRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetFinancialReportQueryHandler(
        IPaymentRepository paymentRepository,
        IExpenseRepository expenseRepository,
        ISalaryPaymentRepository salaryPaymentRepository)
    {
        _paymentRepository = paymentRepository;
        _expenseRepository = expenseRepository;
        _salaryPaymentRepository = salaryPaymentRepository;
    }

    public async Task<Result<List<FinancialReportRow>>> Handle(GetFinancialReportQuery request, CancellationToken ct)
    {
        var revenueRows = await _paymentRepository.GetRevenueByMonthAsync(request.From, request.To, request.BranchId, ct);
        var expenseRows = await _expenseRepository.GetExpensesByMonthAsync(request.From, request.To, request.BranchId, ct);
        var salaryRows = await _salaryPaymentRepository.GetPaidSalariesByMonthAsync(request.From, request.To, request.BranchId, ct);

        var periods = revenueRows.Select(r => r.GroupKey)
            .Concat(expenseRows.Select(r => r.GroupKey))
            .Concat(salaryRows.Select(r => r.GroupKey))
            .Distinct()
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        var revenueByPeriod = revenueRows.ToDictionary(r => r.GroupKey, r => r.Gross - r.Refunded);
        var expensesByPeriod = expenseRows.ToDictionary(r => r.GroupKey, r => r.Total);
        var salariesByPeriod = salaryRows.ToDictionary(r => r.GroupKey, r => r.Total);

        var rows = periods.Select(period =>
        {
            var revenue = revenueByPeriod.GetValueOrDefault(period);
            var expenses = expensesByPeriod.GetValueOrDefault(period);
            var salaries = salariesByPeriod.GetValueOrDefault(period);

            return new FinancialReportRow(period, revenue, expenses, salaries, revenue - expenses - salaries);
        }).ToList();

        return Result<List<FinancialReportRow>>.Success(rows, _operation);
    }
}
