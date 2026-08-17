namespace SportAcademy.Domain.Entities;

public class TraineeMedicalCondition
{
    public int Id { get; set; }
    public int TraineeId { get; set; }
    public string Condition { get; set; } = null!;

    public virtual Trainee Trainee { get; set; } = null!;
}
