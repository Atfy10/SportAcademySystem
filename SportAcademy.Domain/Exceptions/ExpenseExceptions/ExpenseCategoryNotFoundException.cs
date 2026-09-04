using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.ExpenseExceptions
{
    public class ExpenseCategoryNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(ExpenseCategory);

        public ExpenseCategoryNotFoundException(string id) : base(_entity, id) { }

        public ExpenseCategoryNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }
    }
}
