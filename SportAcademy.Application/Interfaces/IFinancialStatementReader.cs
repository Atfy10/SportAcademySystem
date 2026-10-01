using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    // What a received payment was for: the invoice it was applied to. One row per allocation, so a
    // payment that settled two invoices (e.g. two siblings) is two lines, each named.
    public enum IncomeKind { Subscription, Event, Other }

    public record IncomeLineRow(
        DateTime PaidAt, string PaymentNumber, string BranchName, decimal Amount, IncomeKind Kind,
        string? TraineeName, string? PayerName, string? SportName, string? PlanName, string? EventTitle);

    public record RefundLineRow(
        DateTime RefundedAt, string PaymentNumber, PaymentRefundKind Kind, string Reason,
        decimal Amount, string BranchName, string? PayerName);

    public record ExpenseLineRow(
        DateOnly Date, string Title, int CategoryId, string CategoryName, string BranchName,
        decimal Amount, string? Notes);

    public record SalaryLineRow(
        DateTime PaidAt, DateOnly PeriodMonth, string EmployeeName, string BranchName, decimal Amount, decimal Bonus);

    // The individual lines behind the financial report. Each method filters exactly like its
    // monthly counterpart (IPaymentRepository.GetRevenueByMonthAsync, IExpenseRepository.
    // GetExpensesByMonthAsync, ISalaryPaymentRepository.GetPaidSalariesByMonthAsync), so the
    // itemized totals and the monthly totals always agree. Each returns at most `limit` lines,
    // oldest first.
    public interface IFinancialStatementReader
    {
        Task<List<IncomeLineRow>> GetIncomeAsync(DateTime? from, DateTime? to, int? branchId, string lang, int limit, CancellationToken ct = default);
        Task<List<RefundLineRow>> GetRefundsAsync(DateTime? from, DateTime? to, int? branchId, string lang, int limit, CancellationToken ct = default);
        Task<List<ExpenseLineRow>> GetExpensesAsync(DateTime? from, DateTime? to, int? branchId, string lang, int limit, CancellationToken ct = default);
        Task<List<SalaryLineRow>> GetPaidSalariesAsync(DateTime? from, DateTime? to, int? branchId, string lang, int limit, CancellationToken ct = default);
    }
}
