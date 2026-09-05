using FluentValidation;
using SportAcademy.Application.Commands.ExpenseCommands.UpdateExpense;

namespace SportAcademy.Application.Validators.ExpenseValidators
{
    public class UpdateExpenseValidator : AbstractValidator<UpdateExpenseCommand>
    {
        public UpdateExpenseValidator()
        {
            RuleFor(x => x.Id).ApplyIdRuleFor("Expense");

            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
                .When(x => x.Title is not null);

            RuleFor(x => x.ExpenseCategoryId).ApplyOptionalIdRuleFor("Expense category");
            RuleFor(x => x.BranchId).ApplyOptionalIdRuleFor("Branch");
            RuleFor(x => x.PaymentTypeId).ApplyOptionalIdRuleFor("Payment type");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.")
                .When(x => x.Amount.HasValue);

            RuleFor(x => x.ExpenseDate)
                .Must(BeWithinCurrentMonth)
                .WithMessage("Expense date must be within the current month.")
                .When(x => x.ExpenseDate.HasValue);
        }

        private static bool BeWithinCurrentMonth(DateOnly? date)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var firstOfMonth = new DateOnly(today.Year, today.Month, 1);
            return date!.Value >= firstOfMonth && date.Value <= today;
        }
    }
}
