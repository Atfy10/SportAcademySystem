using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.ExpenseExceptions
{
    public class ExpenseNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(Expense);

        public ExpenseNotFoundException(string id) : base(_entity, id) { }

        public ExpenseNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
