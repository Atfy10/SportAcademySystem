using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.CoachDtos;

public record CoachCardDto(
    int Id,
    string FirstName,
    string LastName,
    string Position,
    string BranchName,
    string Email,
    bool IsWork,
    string PhoneNumber,
    string Address,
    DateTime HireDate,
    //int TotalTrainees,
    SkillLevel SkillLevel,
    string Sport
);