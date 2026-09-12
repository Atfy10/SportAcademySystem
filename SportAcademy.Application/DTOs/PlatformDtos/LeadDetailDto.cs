namespace SportAcademy.Application.DTOs.PlatformDtos;

public record LeadDetailDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = default!;
    public string AcademyName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string PhoneNumber { get; init; } = default!;
    public string? City { get; init; }
    public int? BranchCount { get; init; }
    public int? TraineeCountBand { get; init; }
    public string? Message { get; init; }
    public string Locale { get; init; } = default!;
    public string? SourcePage { get; init; }
    public string? UtmSource { get; init; }
    public string? UtmMedium { get; init; }
    public string? UtmCampaign { get; init; }
    public string? Referrer { get; init; }
    public string Status { get; init; } = default!;
    public string? InternalNotes { get; init; }
    public Guid? ConvertedTenantId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ContactedAt { get; init; }
}
