using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ChatDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.ChatCommands.SendMessageToBot
{
    public record SendMessageToBotCommand(
        Guid ConversationId,
        string Message
    ) : IRequest<Result<ChatMessageDto>>;
}
