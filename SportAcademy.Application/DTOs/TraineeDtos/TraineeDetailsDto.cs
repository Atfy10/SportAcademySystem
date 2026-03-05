namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeDetailsDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? ParentNumber,
    string? GuardianName,
    string BranchName,
    DateOnly BirthDate,
    string Gender,
    IReadOnlyList<string>? Sports,
    bool IsSubscribed,
    int EnrollmentCount,
    DateTime JoinDate
)
{
    public double AttendanceRate { get; set; }
}
