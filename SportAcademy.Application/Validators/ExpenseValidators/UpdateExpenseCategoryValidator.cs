using FluentValidation;
using SportAcademy.Application.Commands.ExpenseCategoryCommands.UpdateExpenseCategory;

namespace SportAcademy.Application.Validators.ExpenseValidators
{
    public class UpdateExpenseCategoryValidator : AbstractValidator<UpdateExpenseCategoryCommand>
    {
        public UpdateExpenseCategoryValidator()
        {
            RuleFor(x => x.Id).ApplyIdRuleFor("Expense category");

            RuleFor(x => x.Name)
                .MaximumLength(50).WithMessage("Name must not exceed 50 characters.")
                .When(x => x.Name is not null);
        }
    }
}
