namespace SportAcademy.Application.DTOs.AppUserDtos;

public record MeResponse
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string? PhoneNumber { get; init; }
    public Guid TenantId { get; init; }
    public List<string> Roles { get; init; } = [];
    public DateTime? CreatedAt { get; init; }
}
