namespace SportAcademy.Domain.Entities;

public class ChatConversation
{
    public Guid Id { get; private set; }
    public string? Title { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<OpenAiMessage> Messages { get; set; } = new List<OpenAiMessage>();

    private ChatConversation() { }

    private ChatConversation(Guid id, string? title)
    {
        Id = id;
        Title = title;
        CreatedAt = DateTime.UtcNow;
    }

    public static ChatConversation Create(Guid id, string? title)
        => new(id, title);
}
