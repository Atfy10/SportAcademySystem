using SportAcademy.Application.Commands.ChatCommands.CreateConversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SportAcademy.Application.Commands.ChatCommands;


namespace SportAcademy.Application.Validators.ChatValidators
{
    public class CreateConversationValidator
    : AbstractValidator<CreateConversationCommand>
    {
        public CreateConversationValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Title));
        }
    }
}
