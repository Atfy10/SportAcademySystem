using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Queries.ReportQueries.GetFinancialStatement;

// Groups every line behind the financial report into its basic categories - see
// FinancialStatementDto. Money in is dated when it was received, refunds when they left (never
// netted against the original payment's month), expenses on their expense date, salaries when
// they were paid - the same placement as the monthly report.
public class GetFinancialStatementQueryHandler : IRequestHandler<GetFinancialStatementQuery, Result<FinancialStatementDto>>
{
    // Lines listed per kind (payments, refunds, expenses, salaries) - plenty for any sensible
    // period, and it keeps a years-wide range from shipping every row the academy ever recorded.
    internal const int MaxLinesPerKind = 5000;

    private readonly IFinancialStatementReader _reader;
    private readonly ICurrentLanguageProvider _languageProvider;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ISalaryPaymentRepository _salaryPaymentRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetFinancialStatementQueryHandler(
        IFinancialStatementReader reader,
        ICurrentLanguageProvider languageProvider,
        IPaymentRepository paymentRepository,
        IExpenseRepository expenseRepository,
        ISalaryPaymentRepository salaryPaymentRepository)
    {
        _reader = reader;
        _languageProvider = languageProvider;
        _paymentRepository = paymentRepository;
        _expenseRepository = expenseRepository;
        _salaryPaymentRepository = salaryPaymentRepository;
    }

    public async Task<Result<FinancialStatementDto>> Handle(GetFinancialStatementQuery request, CancellationToken ct)
    {
        var lang = _languageProvider.Language;
        const int fetch = MaxLinesPerKind + 1;
        var income = await _reader.GetIncomeAsync(request.From, request.To, request.BranchId, lang, fetch, ct);
        var refunds = await _reader.GetRefundsAsync(request.From, request.To, request.BranchId, lang, fetch, ct);
        var expenses = await _reader.GetExpensesAsync(request.From, request.To, request.BranchId, lang, fetch, ct);
        var salaries = await _reader.GetPaidSalariesAsync(request.From, request.To, request.BranchId, lang, fetch, ct);

        var truncated = income.Count > MaxLinesPerKind || refunds.Count > MaxLinesPerKind
            || expenses.Count > MaxLinesPerKind || salaries.Count > MaxLinesPerKind;
        income = income.Take(MaxLinesPerKind).ToList();
        refunds = refunds.Take(MaxLinesPerKind).ToList();
        expenses = expenses.Take(MaxLinesPerKind).ToList();
        salaries = salaries.Take(MaxLinesPerKind).ToList();

        var incomeCategories = new List<FinancialStatementCategoryDto>
        {
            Category("subscriptions", "Subscriptions", income
                .Where(r => r.Kind == IncomeKind.Subscription)
                .Select(r => Item(r.PaidAt, r.TraineeName ?? r.PayerName ?? r.PaymentNumber,
                    JoinDetail(r.SportName, r.PlanName), r.PaymentNumber, r.BranchName, r.Amount))),
            Category("events", "Events", income
                .Where(r => r.Kind == IncomeKind.Event)
                .Select(r => Item(r.PaidAt, r.EventTitle ?? r.PayerName ?? r.PaymentNumber,
                    r.EventTitle is null ? null : r.PayerName, r.PaymentNumber, r.BranchName, r.Amount))),
            Category("otherIncome", "Other income", income
                .Where(r => r.Kind == IncomeKind.Other)
                .Select(r => Item(r.PaidAt, r.TraineeName ?? r.PayerName ?? r.PaymentNumber,
                    null, r.PaymentNumber, r.BranchName, r.Amount))),
            Category("refunds", "Refunds", refunds
                .Select(r => Item(r.RefundedAt, r.PayerName ?? r.PaymentNumber, r.Reason, r.PaymentNumber,
                    r.BranchName, -r.Amount, tag: r.Kind == PaymentRefundKind.Void ? "void" : "refund"))),
        };

        var expenseCategories = expenses
            .GroupBy(e => new { e.CategoryId, e.CategoryName })
            .OrderBy(g => g.Key.CategoryName, StringComparer.CurrentCultureIgnoreCase)
            .Select(g => Category($"expense-{g.Key.CategoryId}", g.Key.CategoryName, g
                .Select(e => new FinancialStatementItemDto(e.Date, e.Title, e.Notes, null, e.BranchName, e.Amount))))
            .ToList();

        var salaryCategories = new List<FinancialStatementCategoryDto>
        {
            Category("salaries", "Salaries", salaries
                .Select(s => new FinancialStatementItemDto(
                    DateOnly.FromDateTime(TenantCalendar.ToLocal(s.PaidAt)), s.EmployeeName, null, null, s.BranchName,
                    s.Amount + s.Bonus, Period: s.PeriodMonth, Bonus: s.Bonus > 0 ? s.Bonus : null))),
        };

        var sections = new List<FinancialStatementSectionDto>
        {
            Section("income", incomeCategories),
            Section("expenses", expenseCategories),
            Section("salaries", salaryCategories),
        };

        var totalIncome = sections[0].Total;
        var totalExpenses = sections[1].Total;
        var totalSalaries = sections[2].Total;

        // A cut-off list can't be summed - take the totals from the same aggregates the monthly
        // report uses, so they still cover the whole period.
        if (truncated)
        {
            var revenue = await _paymentRepository.GetRevenueByMonthAsync(request.From, request.To, request.BranchId, ct);
            totalIncome = revenue.Sum(r => r.Gross - r.Refunded);
            totalExpenses = (await _expenseRepository.GetExpensesByMonthAsync(request.From, request.To, request.BranchId, ct)).Sum(r => r.Total);
            totalSalaries = (await _salaryPaymentRepository.GetPaidSalariesByMonthAsync(request.From, request.To, request.BranchId, ct)).Sum(r => r.Total);
        }

        var dto = new FinancialStatementDto(
            request.From,
            request.To,
            new FinancialStatementTotalsDto(totalIncome, totalExpenses, totalSalaries, totalIncome - totalExpenses - totalSalaries),
            sections,
            truncated);

        return Result<FinancialStatementDto>.Success(dto, _operation);
    }

    // Empty categories are dropped, so the report only lists what actually happened.
    private static FinancialStatementSectionDto Section(string key, List<FinancialStatementCategoryDto> categories)
    {
        var present = categories.Where(c => c.Count > 0).ToList();
        return new FinancialStatementSectionDto(key, present.Sum(c => c.Total), present);
    }

    private static FinancialStatementCategoryDto Category(string key, string name, IEnumerable<FinancialStatementItemDto> items)
    {
        var list = items.OrderBy(i => i.Date).ToList();
        return new FinancialStatementCategoryDto(key, name, list.Sum(i => i.Amount), list.Count, list);
    }

    // Payments and refunds are stored as UTC instants; the report shows the academy's day.
    private static FinancialStatementItemDto Item(
        DateTime atUtc, string title, string? detail, string? reference, string branchName, decimal amount, string? tag = null)
        => new(DateOnly.FromDateTime(TenantCalendar.ToLocal(atUtc)), title, detail, reference, branchName, amount, tag);

    private static string? JoinDetail(string? sport, string? plan)
        => sport is null ? plan : plan is null ? sport : $"{sport} – {plan}";
}
