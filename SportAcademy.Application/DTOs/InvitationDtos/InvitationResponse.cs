namespace SportAcademy.Application.DTOs.InvitationDtos;

public record InvitationResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = default!;
    public string Status { get; init; } = default!;
    public bool IsExpired { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }

    // Only ever populated by the handler that just minted the raw token this links to
    // (Invitation.TokenHash is one-way, so no other read path can ever reconstruct this) - lets
    // the caller show a "Copy Link" / "Send via Email" choice right after creation, since nothing
    // is auto-emailed on creation any more. Null on any response built after the fact (e.g. a
    // list/detail read), where no raw token is available to build it from.
    public string? InviteUrl { get; init; }
}
