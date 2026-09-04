namespace SportAcademy.Domain.Exceptions.ExpenseExceptions
{
    public class ExpenseCategoryInUseException : Exception
    {
        public ExpenseCategoryInUseException(int expenseCategoryId)
            : base($"Expense category {expenseCategoryId} is used by one or more recorded expenses and cannot be " +
                   "deleted. Deactivate it instead to hide it from new expenses while preserving history.")
        { }
    }
}
