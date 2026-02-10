using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace SportAcademy.Application.Commands.ChatCommands.CreateConversation
{
    public record CreateConversationCommand(
    string? Title
) : IRequest<Result<ChatConversationDto>>;
}
