namespace SportAcademy.Domain.Exceptions.TraineeExceptions
{
    public sealed class InvalidFamilyMemberNumberException : Exception
    {
        public InvalidFamilyMemberNumberException(int memberNumber)
            : base($"Family member number '{memberNumber}' must be between 0 and 9999.")
        {
        }
    }
}
