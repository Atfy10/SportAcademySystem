using FluentValidation;
using SportAcademy.Application.Commands.ExpenseCommands.CreateExpense;

namespace SportAcademy.Application.Validators.ExpenseValidators
{
    public class CreateExpenseValidator : AbstractValidator<CreateExpenseCommand>
    {
        public CreateExpenseValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.ExpenseCategoryId).ApplyIdRuleFor("Expense category");
            RuleFor(x => x.BranchId).ApplyIdRuleFor("Branch");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.PaymentTypeId).ApplyOptionalIdRuleFor("Payment type");
        }
    }
}
