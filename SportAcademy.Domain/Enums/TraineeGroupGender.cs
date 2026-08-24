namespace SportAcademy.Domain.Enums
{
    // Deliberately separate from Gender (a person's own gender, Male/Female only) - a
    // TraineeGroup's gender policy can additionally be Mixed, which must never be a
    // selectable value for a Trainee/Employee/AppUser's own Gender.
    public enum TraineeGroupGender
    {
        Male = 1,
        Female = 2,
        Mixed = 3
    }
}
