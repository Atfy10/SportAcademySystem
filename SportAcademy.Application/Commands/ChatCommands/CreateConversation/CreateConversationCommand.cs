using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Services;
using MediatR;

namespace SportAcademy.Application.Commands.ChatCommands.CreateConversation
{
    public record CreateConversationCommand(string? Title) : IRequest<Result<ChatConversationDto>>;
}
