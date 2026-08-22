namespace SportAcademy.Application.DTOs.FamilyDtos;

public record FamilyMemberDto(
    int Id,
    string Code,
    string FirstName,
    string LastName,
    int Age,
    string PhoneNumber,
    bool IsSubscribed,
    string? BranchName
);
