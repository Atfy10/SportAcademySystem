using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }

    public Guid ChatConversationId { get; set; }

    public ChatRole Role { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    // Navigation Property
    public ChatConversation? ChatConversation { get; set; }
}
