namespace SportAcademy.Domain.Enums
{
    // A group's commercial type. Fixes three things that differ between the two: the price a
    // trainee pays (SportPrice is keyed by this alongside sport/branch/subscription type), how
    // many trainees the group may hold (Private groups are capped far lower - see the
    // TraineeGroup validators), and which groups a given subscription may be enrolled into (a
    // subscription records the type it was priced for, and only matching groups are offered).
    public enum TraineeGroupType
    {
        Public = 1,
        Private = 2
    }
}
