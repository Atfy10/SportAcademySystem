namespace SportAcademy.Application.DTOs.BranchDtos;

public record BranchStatsDto
{
    public int TotalTrainees { get; init; }
    public int TotalCoaches { get; init; }
    public int ActiveGroups { get; init; }
    public int ActiveSessions { get; init; }
}
