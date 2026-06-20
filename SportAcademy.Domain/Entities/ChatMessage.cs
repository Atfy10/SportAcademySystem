using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities;

public class OpenAiMessage
{
    public Guid Id { get; private set; }
    public Guid ChatConversationId { get; private set; }
    public ChatRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public ChatConversation? ChatConversation { get; set; }

    private OpenAiMessage() { }

    private OpenAiMessage(Guid id, Guid chatConversationId, ChatRole role, string content)
    {
        Id = id;
        ChatConversationId = chatConversationId;
        Role = role;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

        public static OpenAiMessage Create(Guid id, Guid chatConversationId, ChatRole role, string content)
            => new(id, chatConversationId, role, content);

        public static OpenAiMessage CreateForChat(ChatRole role, string content)
            => new OpenAiMessage { Role = role, Content = content };
}
