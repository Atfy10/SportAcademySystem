namespace SportAcademy.Application.DTOs.CoachDtos;

public record CoachDetailsDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string BranchName,
    string SportName,
    string SkillLevel,
    string[]? Certifications,
    int? TotalTrainees,
    DateTime? HireDate,
    bool IsWork,
    int? Rating
);
