namespace SportAcademy.Application.DTOs.AppUserDtos;

public record AppUserCardDto
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = default!;

    public string Email { get; init; } = default!;

    public List<string> Roles { get; init; } = [];

    public bool IsActive { get; init; }
}
