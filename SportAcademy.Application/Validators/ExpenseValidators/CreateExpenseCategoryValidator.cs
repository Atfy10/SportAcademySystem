using FluentValidation;
using SportAcademy.Application.Commands.ExpenseCategoryCommands.CreateExpenseCategory;

namespace SportAcademy.Application.Validators.ExpenseValidators
{
    public class CreateExpenseCategoryValidator : AbstractValidator<CreateExpenseCategoryCommand>
    {
        public CreateExpenseCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");
        }
    }
}
