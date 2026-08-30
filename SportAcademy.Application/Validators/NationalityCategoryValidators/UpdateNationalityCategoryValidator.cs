using FluentValidation;
using SportAcademy.Application.Commands.NationalityCategoryCommands.UpdateNationalityCategory;

namespace SportAcademy.Application.Validators.NationalityCategoryValidators
{
    public class UpdateNationalityCategoryValidator : AbstractValidator<UpdateNationalityCategoryCommand>
    {
        public UpdateNationalityCategoryValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Please provide a valid nationality category ID.");

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
