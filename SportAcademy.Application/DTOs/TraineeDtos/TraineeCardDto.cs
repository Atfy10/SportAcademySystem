namespace SportAcademy.Application.DTOs.TraineeDtos;

public record TraineeCardDto(
    int Id,
    string FirstName,
    string LastName,
    int Age,
    string Email,
    string PhoneNumber,
    DateTime JoinDate,
    bool IsSubscribed,
    IReadOnlyList<TraineeSportSkillDto> SportSkills,
    string? CoachName,
    string? BranchName
)
{
    public double AttendanceRate { get; set; }
}