using SportAcademy.Application.DTOs.ExpenseDtos;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written Expense -> ExpenseDto mapping, shared by Create/Update/GetExpenseById -
    // all three need the branch/category/payment-type names, which only exist once the entity
    // is (re)loaded with its includes.
    public static class ExpenseMapper
    {
        public static ExpenseDto ToDto(Expense e) => new(
            e.Id, e.Title, e.ExpenseCategoryId, e.ExpenseCategory?.Name ?? string.Empty,
            e.BranchId, e.Branch?.Name ?? string.Empty, e.Amount, e.Currency, e.ExpenseDate,
            e.PaymentTypeId, e.PaymentType?.Name, e.Notes, e.RecordedByUserId, e.CreatedAt);
    }
}
