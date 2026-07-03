namespace SportAcademy.Application.DTOs.PlatformDtos;

public record TenantListResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public string Code { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Status { get; init; } = default!;
    public string? PlanName { get; init; }
    public DateTime CreatedAt { get; init; }
    public int BranchCount { get; init; }
    public int UserCount { get; init; }
}
