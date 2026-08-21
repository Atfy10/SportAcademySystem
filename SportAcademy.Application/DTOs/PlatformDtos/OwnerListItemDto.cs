namespace SportAcademy.Application.DTOs.PlatformDtos;

public record OwnerListItemDto
{
    public Guid Id { get; init; }
    public string? UserName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsBanned { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = default!;
    public string TenantStatus { get; init; } = default!;
}
