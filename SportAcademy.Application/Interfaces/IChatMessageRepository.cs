using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces;

public interface IChatMessageRepository
{
    Task AddAsync(ChatMessage message, CancellationToken cancellationToken);

    Task<IReadOnlyList<ChatMessage>> GetByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken);
}
