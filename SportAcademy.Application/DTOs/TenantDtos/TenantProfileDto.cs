namespace SportAcademy.Application.DTOs.TenantDtos;

public record TenantProfileDto
{
    public string OrganizationName { get; init; } = default!;
    public string? LogoUrl { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    public string? Address { get; init; }
    public string? TaxNumber { get; init; }
    public string? CommercialRegistration { get; init; }
    public string? Description { get; init; }
}
