namespace SportAcademy.Application.DTOs.TenantDtos;

public record CurrentTenantResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public string Status { get; init; } = default!;
}
