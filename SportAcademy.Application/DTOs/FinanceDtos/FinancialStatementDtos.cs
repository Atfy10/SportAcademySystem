namespace SportAcademy.Application.DTOs.FinanceDtos;

// The financial report, itemized: every amount behind the income / expenses / salaries totals,
// each named (who paid and for what, what was bought, who was paid) and grouped into a few basic
// categories. Totals match GetFinancialReport's monthly figures for the same filters exactly -
// it reads the same payments, refunds, expenses and paid salaries.
//
// Section keys: "income", "expenses", "salaries". Category keys: "subscriptions", "events",
// "otherIncome", "refunds", "salaries", and "expense-{id}" for each expense category (its Name is
// the academy's own category name; the fixed keys carry an English Name as a fallback only).
// Truncated: a kind of entry had more lines than the statement lists (see
// GetFinancialStatementQueryHandler.MaxLinesPerKind) - its categories show only the first ones,
// while Totals still cover everything in the period.
public record FinancialStatementDto(
    DateTime? From,
    DateTime? To,
    FinancialStatementTotalsDto Totals,
    List<FinancialStatementSectionDto> Sections,
    bool Truncated = false);

public record FinancialStatementTotalsDto(decimal Income, decimal Expenses, decimal Salaries, decimal Net);

public record FinancialStatementSectionDto(string Key, decimal Total, List<FinancialStatementCategoryDto> Categories);

public record FinancialStatementCategoryDto(
    string Key, string Name, decimal Total, int Count, List<FinancialStatementItemDto> Items);

// Date is the academy's calendar day. Amount is negative for a refund. Tag: "refund" / "void" on
// a refund line. Period/Bonus are set on a salary line (the month it paid for, and the bonus
// included in Amount). Reference is the payment number where there is one.
public record FinancialStatementItemDto(
    DateOnly Date,
    string Title,
    string? Detail,
    string? Reference,
    string BranchName,
    decimal Amount,
    string? Tag = null,
    DateOnly? Period = null,
    decimal? Bonus = null);

// Flat CSV form: one line per item, with its section and category.
public record FinancialStatementCsvRow(
    string Section, string Category, DateOnly Date, string Title, string? Detail,
    string? Reference, string Branch, decimal Amount);
