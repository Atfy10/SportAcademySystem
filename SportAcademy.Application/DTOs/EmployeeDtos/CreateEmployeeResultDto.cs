namespace SportAcademy.Application.DTOs.EmployeeDtos;

public record CreateEmployeeResultDto(int EmployeeId, string? GeneratedUserName, string? GeneratedPassword);
