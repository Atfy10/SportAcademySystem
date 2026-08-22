namespace SportAcademy.Application.DTOs.FinanceDtos;

// GroupKey is either a "yyyy-MM" month label or a branch name, depending on the request's
// GroupBy. Sport-level grouping is a natural follow-up (same data, one more join through
// InvoiceLine.SubscriptionDetails.SportId) but isn't implemented in this pass.
public record RevenueReportRow(string GroupKey, decimal GrossAmount, decimal RefundedAmount, decimal NetAmount, int PaymentCount);

public record OutstandingReportSummary(decimal TotalOutstanding, int InvoiceCount, int OverdueCount, decimal OverdueAmount);

public record PaymentMethodReportRow(string Method, decimal TotalAmount, int PaymentCount);
