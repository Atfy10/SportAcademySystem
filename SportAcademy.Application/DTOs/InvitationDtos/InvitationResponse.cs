namespace SportAcademy.Application.DTOs.InvitationDtos;

public record InvitationResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = default!;
    public string Status { get; init; } = default!;
    public bool IsExpired { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
