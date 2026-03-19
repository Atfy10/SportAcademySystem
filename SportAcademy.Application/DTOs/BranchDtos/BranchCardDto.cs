namespace SportAcademy.Application.DTOs.BranchDtos;

public record BranchCardDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public string City { get; init; } = default!;
    public string Country { get; init; } = default!;
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public double? CoX { get; init; }
    public double? CoY { get; init; }
}
