namespace SportAcademy.Application.DTOs.PlatformDtos
{
    public record TenantAuditEventDto(
        string Id,
        Guid TenantId,
        string Type,
        string Message,
        DateTime At,
        string? Actor
    );
}
