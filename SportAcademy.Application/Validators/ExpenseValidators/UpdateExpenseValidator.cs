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
        }
    }
}
