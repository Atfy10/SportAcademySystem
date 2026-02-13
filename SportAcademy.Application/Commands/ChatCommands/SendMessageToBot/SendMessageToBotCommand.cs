using MediatR;
using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Services;
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
