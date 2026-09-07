namespace SportAcademy.Application.DTOs.PlatformDtos
{
    public record ImpersonationSessionDto(
        string AccessToken,
        Guid GrantId,
        Guid TenantId,
        string TenantDisplayName,
        DateTime ExpiresAt);
}
