namespace SportAcademy.Application.DTOs.TenantDtos;

public record CurrentTenantResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public string Status { get; init; } = default!;
    // The academy's own branding (Settings > General), returned with the identity so the console
    // shows name and logo from one call - the same call after login, after a token refresh and
    // after a page reload - instead of two independently-failing ones.
    public string? LogoUrl { get; init; }
    public string? OrganizationName { get; init; }
}
