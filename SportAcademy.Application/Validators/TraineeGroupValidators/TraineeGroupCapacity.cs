namespace SportAcademy.Application.Validators.TraineeGroupValidators
{
    /// <summary>
    /// Capacity ceiling for private groups, shared by the create and update validators so the
    /// two can't drift apart. A business limit an academy may well want tuned - one place to
    /// change it.
    /// <para>
    /// Deliberately does NOT cover the public ceiling: create allows 50 and update allows 15
    /// today. That difference pre-dates this feature and is left exactly as it was rather than
    /// silently changed here.
    /// </para>
    /// </summary>
    public static class TraineeGroupCapacity
    {
        public const int PrivateMaximum = 8;
        public const int PrivateDefault = 4;
    }
}
