namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeDropdownDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
}
