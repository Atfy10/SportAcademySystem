using FluentAssertions;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.ReportQueries.GetFinancialStatement;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class GetFinancialStatementQueryHandlerTests
{
    private readonly Mock<IFinancialStatementReader> _reader = new();
    private readonly Mock<ICurrentLanguageProvider> _language = new();
    private readonly GetFinancialStatementQueryHandler _handler;

    public GetFinancialStatementQueryHandlerTests()
    {
        _language.SetupGet(l => l.Language).Returns("en");
        _handler = new GetFinancialStatementQueryHandler(
            _reader.Object, _language.Object,
            new Mock<IPaymentRepository>().Object, new Mock<IExpenseRepository>().Object, new Mock<ISalaryPaymentRepository>().Object);

        var at = new DateTime(2026, 9, 10, 9, 0, 0, DateTimeKind.Utc);
        _reader.Setup(r => r.GetIncomeAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), "en", It.IsAny<int>(), default))
            .ReturnsAsync([
                new IncomeLineRow(at, "PAY-2026-00001", "Main", 45m, IncomeKind.Subscription, "Ahmad Ali", null, "Football", "Monthly", null),
                new IncomeLineRow(at, "PAY-2026-00002", "Main", 30m, IncomeKind.Subscription, "Sara Ali", null, "Swimming", "Monthly", null),
                new IncomeLineRow(at, "PAY-2026-00003", "Main", 200m, IncomeKind.Event, null, "Omar Saleh", null, null, "Birthday party"),
            ]);
        _reader.Setup(r => r.GetRefundsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), "en", It.IsAny<int>(), default))
            .ReturnsAsync([new RefundLineRow(at, "PAY-2026-00002", PaymentRefundKind.Refund, "Moved away", 10m, "Main", "Sara Ali")]);
        _reader.Setup(r => r.GetExpensesAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), "en", It.IsAny<int>(), default))
            .ReturnsAsync([
                new ExpenseLineRow(new DateOnly(2026, 9, 1), "September rent", 2, "Rent", "Main", 100m, null),
                new ExpenseLineRow(new DateOnly(2026, 9, 5), "Water bill", 1, "Utilities", "Main", 15m, null),
                new ExpenseLineRow(new DateOnly(2026, 9, 6), "Electricity", 1, "Utilities", "Main", 25m, null),
            ]);
        _reader.Setup(r => r.GetPaidSalariesAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), "en", It.IsAny<int>(), default))
            .ReturnsAsync([new SalaryLineRow(at, new DateOnly(2026, 9, 1), "Khaled Coach", "Main", 60m, 5m)]);
    }

    [Fact]
    public async Task Handle_GroupsEveryLineIntoItsCategory_WithNamesAndTotals()
    {
        var result = await _handler.Handle(new GetFinancialStatementQuery(null, null, null), default);
        var statement = result.Data!;

        var income = statement.Sections.Single(s => s.Key == "income");
        income.Categories.Select(c => c.Key).Should().Equal("subscriptions", "events", "refunds");

        var subscriptions = income.Categories.Single(c => c.Key == "subscriptions");
        subscriptions.Total.Should().Be(75m);
        subscriptions.Items.Select(i => i.Title).Should().Equal("Ahmad Ali", "Sara Ali");
        subscriptions.Items[0].Detail.Should().Be("Football – Monthly");

        var ev = income.Categories.Single(c => c.Key == "events").Items.Single();
        ev.Title.Should().Be("Birthday party");
        ev.Detail.Should().Be("Omar Saleh");

        var refund = income.Categories.Single(c => c.Key == "refunds").Items.Single();
        refund.Amount.Should().Be(-10m);
        refund.Tag.Should().Be("refund");

        var expenses = statement.Sections.Single(s => s.Key == "expenses");
        expenses.Categories.Select(c => c.Name).Should().Equal("Rent", "Utilities");
        expenses.Categories.Single(c => c.Name == "Utilities").Total.Should().Be(40m);

        var salary = statement.Sections.Single(s => s.Key == "salaries").Categories.Single().Items.Single();
        salary.Title.Should().Be("Khaled Coach");
        salary.Amount.Should().Be(65m);
        salary.Bonus.Should().Be(5m);
        salary.Period.Should().Be(new DateOnly(2026, 9, 1));

        statement.Totals.Income.Should().Be(265m);
        statement.Totals.Expenses.Should().Be(140m);
        statement.Totals.Salaries.Should().Be(65m);
        statement.Totals.Net.Should().Be(60m);
    }

    [Fact]
    public async Task Handle_LeavesOutCategoriesWithNothingInThem()
    {
        var result = await _handler.Handle(new GetFinancialStatementQuery(null, null, null), default);

        result.Data!.Sections.Single(s => s.Key == "income").Categories
            .Should().NotContain(c => c.Key == "otherIncome");
    }
}
