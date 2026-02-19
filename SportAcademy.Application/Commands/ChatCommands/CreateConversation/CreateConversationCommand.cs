using SportAcademy.Application.DTOs.ChatDtos;
using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.ChatCommands.CreateConversation
{
    public record CreateConversationCommand(string? Title) : IRequest<Result<ChatConversationDto>>;
}
