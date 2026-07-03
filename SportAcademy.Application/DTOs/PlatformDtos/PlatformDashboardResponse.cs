namespace SportAcademy.Application.DTOs.PlatformDtos;

public record PlatformDashboardResponse
{
    public int TotalTenants { get; init; }
    public int ActiveCount { get; init; }
    public int PendingCount { get; init; }
    public int SuspendedCount { get; init; }
    public int ArchivedCount { get; init; }
    public int TotalUsers { get; init; }
    public int TotalBranches { get; init; }
}
