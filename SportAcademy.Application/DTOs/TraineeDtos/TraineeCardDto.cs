namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeCardDto(
    int Id,
    string FirstName,
    string LastName,
    int Age,
    string Email,
    string PhoneNumber,
    DateOnly JoinDate,
    bool IsSubscribed,
    string SportName,
    string CoachName,
    string SkillLevel,
    string BranchName
)
{
    public double AttendanceRate { get; set; }
}