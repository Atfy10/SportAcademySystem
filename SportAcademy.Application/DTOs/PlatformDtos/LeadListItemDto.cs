namespace SportAcademy.Application.DTOs.PlatformDtos;

public record LeadListItemDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = default!;
    public string AcademyName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string PhoneNumber { get; init; } = default!;
    public string? City { get; init; }
    public int? BranchCount { get; init; }
    public string? SourcePage { get; init; }
    public string? UtmSource { get; init; }
    public string Status { get; init; } = default!;
    public Guid? ConvertedTenantId { get; init; }
    public DateTime CreatedAt { get; init; }
}
