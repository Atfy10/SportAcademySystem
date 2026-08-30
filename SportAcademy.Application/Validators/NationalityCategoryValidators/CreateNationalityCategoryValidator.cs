using FluentValidation;
using SportAcademy.Application.Commands.NationalityCategoryCommands.CreateNationalityCategory;

namespace SportAcademy.Application.Validators.NationalityCategoryValidators
{
    public class CreateNationalityCategoryValidator : AbstractValidator<CreateNationalityCategoryCommand>
    {
        public CreateNationalityCategoryValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(3).WithMessage("Code must not exceed 3 characters.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.NameAr)
                .MaximumLength(100).WithMessage("Arabic name must not exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.NameAr));
        }
    }
}
