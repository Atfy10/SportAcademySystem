using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.SportDtos;

public record SportDto(
    int Id,
    string Name,
    string? Description,
    SportCategory Category,
    bool IsRequireHealthTest
);
