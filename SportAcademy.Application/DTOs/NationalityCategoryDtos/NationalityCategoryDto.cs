namespace SportAcademy.Application.DTOs.NationalityCategoryDtos;

public record NationalityCategoryDto
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
}
