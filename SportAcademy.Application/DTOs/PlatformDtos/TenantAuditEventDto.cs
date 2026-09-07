namespace SportAcademy.Application.DTOs.PlatformDtos
{
    public record TenantAuditEventDto(
        string Id,
        Guid? TenantId,
        string Type,
        string Message,
        DateTime At,
        string? Actor,
        Guid? PerformedByUserId,
        string Outcome,
        string? Reason,
        string? AfterJson,
        string? BeforeJson,
        string? IpAddress,
        string? UserAgent,
        string? CorrelationId
    );
}
