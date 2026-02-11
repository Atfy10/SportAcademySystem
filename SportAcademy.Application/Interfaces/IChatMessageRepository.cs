using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces;

public interface IChatMessageRepository : IBaseRepository<ChatMessage, Guid>
{
    Task<IReadOnlyList<ChatMessage>> GetByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken);
}
