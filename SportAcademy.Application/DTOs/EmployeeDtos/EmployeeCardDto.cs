namespace SportAcademy.Application.DTOs.EmployeeDtos;

public record EmployeeCardDto(
    int Id,
    string FirstName,
    string LastName,
    string Position,
    string BranchName,
    string Email,
    bool IsWork,
    string PhoneNumber,
    string Address,
    DateTime HireDate
);