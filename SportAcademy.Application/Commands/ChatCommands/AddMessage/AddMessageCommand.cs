using SportAcademy.Application.DTOs.ChatDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Domain.Enums;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.ChatCommands.AddMessage
{
    public record AddMessageCommand(
        Guid ConversationId,
        ChatRole Role,
        string Content
    ) : IRequest<Result<ChatMessageDto>>;
}
