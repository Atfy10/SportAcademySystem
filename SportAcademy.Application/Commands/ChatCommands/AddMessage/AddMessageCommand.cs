using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.ChatCommands.AddMessage
{
    public record AddMessageCommand(
        Guid ConversationId,
        ChatRole Role,
        string Content
    ) : IRequest<Result<ChatMessageDto>>;
}
