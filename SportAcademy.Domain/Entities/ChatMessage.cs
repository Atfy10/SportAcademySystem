using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities;

public class OpenAiMessage : ITenantScoped
{
    public Guid Id { get; set; }

    public Guid ChatConversationId { get; set; }

    public ChatRole Role { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Navigation Property
    public ChatConversation? ChatConversation { get; set; }
}
