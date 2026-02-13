using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces;

public interface IChatMessageRepository : IBaseRepository<OpenAiMessage, Guid>
{
    Task<IReadOnlyList<OpenAiMessage>> GetByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken);
}
