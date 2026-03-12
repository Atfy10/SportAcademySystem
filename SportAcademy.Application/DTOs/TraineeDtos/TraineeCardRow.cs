namespace SportAcademy.Application.DTOs.TraineeDtos;

public class TraineeCardRow
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int Age { get; set; }
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public DateTime JoinDate { get; set; }
    public bool IsSubscribed { get; set; }
    public string? SportSkills { get; set; }
    public string? CoachName { get; set; }
    public string? BranchName { get; set; }
}
