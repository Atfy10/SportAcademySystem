namespace SportAcademy.Application.DTOs.LimitDtos;

// The open (or most recently completed) reconciliation task for one tenant - read by both the
// tenant-side wizard (GetMyLimitReconciliationQuery) and the platform console
// (GetOpenReconciliationsQuery, TenantDetailsPage).
public record LimitReconciliationResponse
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string? TenantDisplayName { get; init; }
    public DateTime OpenedAt { get; init; }
    public DateTime DeadlineAt { get; init; }
    public bool IsCompleted { get; init; }

    /// <summary>True if this reconciliation closed via the SuperAdmin force-reactivate "refuge"
    /// (ConfirmReconciliationBypassCommand) rather than the tenant actually completing its
    /// selection - the tenant may still be over its limit(s).</summary>
    public bool WasBypassedBySuperAdmin { get; init; }

    /// <summary>resourceKey -> the used count that was over cap at the moment this opened.</summary>
    public Dictionary<string, int> RequiredResources { get; init; } = [];
}
