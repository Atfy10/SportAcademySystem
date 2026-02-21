using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeSportDto(
    string SportName,
    SkillLevel SkillLevel,
    IReadOnlyList<string> Branchs
);