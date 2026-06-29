using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities;

public class ChatConversation : ITenantScoped
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Navigation Property
    public ICollection<OpenAiMessage> Messages { get; set; } = new List<OpenAiMessage>();
}
