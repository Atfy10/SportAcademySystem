namespace SportAcademy.Application.DTOs.PlatformDtos;

public record OwnerDetailDto
{
    public Guid Id { get; init; }
    public string? UserName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsBanned { get; init; }
    public bool EmailConfirmed { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = default!;
    public string TenantDisplayName { get; init; } = default!;
    public string TenantStatus { get; init; } = default!;
    public string TenantSlug { get; init; } = default!;
}
