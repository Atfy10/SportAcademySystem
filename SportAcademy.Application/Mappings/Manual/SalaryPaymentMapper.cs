using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written SalaryPayment -> SalaryPaymentDto mapping. nameLookup resolves the three
    // audit-actor ids (CreatedBy/ReviewedBy/PaidBy) to a display name - callers pre-populate it
    // (one IUserRepository.GetDisplayNameAsync call per DISTINCT id across the whole result set,
    // not per row) so a paged list doesn't re-resolve the same accountant's name N times.
    public static class SalaryPaymentMapper
    {
        public static SalaryPaymentDto ToDto(SalaryPayment sp, IReadOnlyDictionary<Guid, string> nameLookup) => new(
            sp.Id,
            sp.EmployeeId,
            sp.Employee is not null ? $"{sp.Employee.FirstName} {sp.Employee.LastName}" : string.Empty,
            sp.BranchId,
            sp.Branch?.Name ?? string.Empty,
            sp.Amount,
            sp.Bonus,
            sp.Currency,
            sp.PeriodMonth,
            sp.Status,
            sp.PaymentTypeId,
            sp.PaymentType?.Name,
            sp.Notes,
            sp.CreatedByUserId,
            sp.CreatedAt,
            sp.ReviewedByUserId,
            sp.ReviewedByUserId.HasValue ? nameLookup.GetValueOrDefault(sp.ReviewedByUserId.Value) : null,
            sp.ReviewedAt,
            sp.RejectionReason,
            sp.PaidByUserId,
            sp.PaidByUserId.HasValue ? nameLookup.GetValueOrDefault(sp.PaidByUserId.Value) : null,
            sp.PaidAt);
    }
}
