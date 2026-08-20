namespace SportAcademy.Application.DTOs.AppUserDtos;

public record AppUserCardDto
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = default!;

    public string Email { get; init; } = default!;

    public List<string> Roles { get; init; } = [];

    // Individual permission grants only - what the user's role(s) already imply is not
    // repeated here, since that's derivable from Roles + the (static) role/permission map.
    public List<string> Permissions { get; init; } = [];

    public bool IsActive { get; init; }
}
