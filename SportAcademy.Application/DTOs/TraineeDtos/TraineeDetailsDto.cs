namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeDetailsDto(
    int Id,
    string Code,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? ParentNumber,
    string? GuardianName,
    string BranchName,
    int BranchId,
    DateOnly BirthDate,
    string Gender,
    IReadOnlyList<string>? Sports,
    IReadOnlyList<int>? SportIds,
    bool IsSubscribed,
    int EnrollmentCount,
    DateTime JoinDate,
    string? ImageUrl
)
{
    public double AttendanceRate { get; set; }
    public List<string> MedicalConditions { get; set; } = [];
}
