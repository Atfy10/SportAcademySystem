using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Contract;

public interface IChatConversationRepository
{
    Task<ChatConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ChatConversation conversation, CancellationToken cancellationToken);
}
