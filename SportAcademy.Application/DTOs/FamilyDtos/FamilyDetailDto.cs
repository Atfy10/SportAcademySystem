namespace SportAcademy.Application.DTOs.FamilyDtos;

public record FamilyDetailDto(
    int Id,
    int Code,
    string? Name,
    string? GuardianName,
    string? GuardianPhone,
    IReadOnlyList<FamilyMemberDto> Members
);
