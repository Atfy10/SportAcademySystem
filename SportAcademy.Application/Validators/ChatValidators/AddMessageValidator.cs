using SportAcademy.Application.Commands.ChatCommands.AddMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SportAcademy.Application.Commands.ChatCommands;


namespace SportAcademy.Application.Validators.ChatValidators
{
    public class AddMessageValidator
    : AbstractValidator<AddMessageCommand>
    {
        public AddMessageValidator()
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty();

            RuleFor(x => x.Content)
                .NotEmpty()
                .MaximumLength(2000);
        }
    }
}
