namespace SportAcademy.Domain.Exceptions.AttendanceExceptions
{
    public class ExcusedRequiresApprovalException : Exception
    {
        public ExcusedRequiresApprovalException()
            : base("Excused can't be marked directly - file an excuse request for Owner/Admin approval instead.")
        {
        }
    }
}
