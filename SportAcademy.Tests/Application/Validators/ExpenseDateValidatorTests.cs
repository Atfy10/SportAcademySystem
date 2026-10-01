using FluentAssertions;
using SportAcademy.Application.Commands.ExpenseCommands.CreateExpense;
using SportAcademy.Application.Commands.ExpenseCommands.UpdateExpense;
using SportAcademy.Application.Validators.ExpenseValidators;
using SportAcademy.Domain.Services;

namespace SportAcademy.Tests.Application.Validators;

// "Today" and "this month" for an expense are the academy's (TenantCalendar.Today, its own time
// zone), not UTC's. Pinned at 1 Oct in the academy - e.g. 00:30 in Kuwait, when UTC is still
// 30 Sep - which is exactly when the UTC version rejected the day's expenses.
public class ExpenseDateValidatorTests
{
    private static readonly DateOnly AcademyToday = new(2026, 10, 1);

    private static CreateExpenseCommand Create(DateOnly date) =>
        new("Water bill", ExpenseCategoryId: 1, BranchId: 1, Amount: 10m, ExpenseDate: date, PaymentTypeId: null, Notes: null);

    private static bool IsValid(DateOnly date)
    {
        TenantCalendar.Set(AcademyToday);
        try
        {
            return new CreateExpenseValidator().Validate(Create(date)).IsValid;
        }
        finally
        {
            TenantCalendar.Set(null);
        }
    }

    [Fact]
    public void Create_TodayOnTheAcademysCalendar_IsAccepted()
        => IsValid(AcademyToday).Should().BeTrue();

    [Fact]
    public void Create_LastMonthOnTheAcademysCalendar_IsRejected()
        => IsValid(new DateOnly(2026, 9, 30)).Should().BeFalse();

    [Fact]
    public void Create_TomorrowOnTheAcademysCalendar_IsRejected()
        => IsValid(new DateOnly(2026, 10, 2)).Should().BeFalse();

    [Fact]
    public void Update_TodayOnTheAcademysCalendar_IsAccepted()
    {
        TenantCalendar.Set(AcademyToday);
        try
        {
            var command = new UpdateExpenseCommand(1, null, null, null, null, AcademyToday, null, null);
            new UpdateExpenseValidator().Validate(command).IsValid.Should().BeTrue();
        }
        finally
        {
            TenantCalendar.Set(null);
        }
    }
}
