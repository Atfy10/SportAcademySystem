namespace SportAcademy.Domain.Exceptions.TraineeExceptions
{
    public sealed class InvalidBranchIdException : Exception
    {
        public InvalidBranchIdException(int branchId)
            : base($"BranchId '{branchId}' is outside the allowed range (0-999).")
        {
        }
    }
}
