using FluentValidation;
using SportAcademy.Application.Commands.UserCommands.UserCreate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Validators.UserValidators
{
    public class CreateUserValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.UserName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Please enter a username.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.")
                // Unicode-aware: char.IsLetterOrDigit accepts Arabic script, unlike the previous
                // ^[a-zA-Z0-9_]+$ regex which rejected every Arabic username outright.
                .Must(u => u is not null && u.All(c => char.IsLetterOrDigit(c) || c == '_'))
                .WithMessage("Username can only contain letters, numbers, and underscores.");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Please enter your email address.")
                .EmailAddress().WithMessage("Please enter a valid email address.");

            RuleFor(x => x.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Please enter your phone number.")
                .Matches(@"^(\+965)?[2569]\d{7}$")
                .WithMessage("Please enter a valid Kuwait phone number (e.g., +96551234567).");
        }
    }
}
