namespace SportAcademy.Domain.Exceptions.SalaryPaymentExceptions
{
    // The approve/reject/mark-paid/delete workflow only allows specific from-states (see
    // SalaryPayment.Status) - this covers every illegal jump, e.g. approving something already
    // Paid, or marking a still-PendingApproval payment as paid.
    public class InvalidSalaryPaymentTransitionException : Exception
    {
        public InvalidSalaryPaymentTransitionException(int id, string currentStatus, string attemptedAction)
            : base($"Salary payment {id} is {currentStatus} and cannot be {attemptedAction}.")
        { }
    }
}
