using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written SubscriptionDiscountRequest -> SubscriptionDiscountRequestDto mapping.
    // nameLookup resolves the two audit-actor ids (RequestedBy/ReviewedBy) to a display name -
    // callers pre-populate it (one IUserRepository.GetDisplayNameAsync call per DISTINCT id
    // across the whole result set), same convention as SalaryPaymentMapper.
    public static class SubscriptionDiscountRequestMapper
    {
        public static SubscriptionDiscountRequestDto ToDto(
            SubscriptionDiscountRequest r, IReadOnlyDictionary<Guid, string> nameLookup) => new(
            r.Id,
            r.TraineeId,
            r.Trainee is not null ? $"{r.Trainee.FirstName} {r.Trainee.LastName}" : string.Empty,
            r.SubscriptionTypeId,
            r.SubscriptionType?.Name ?? string.Empty,
            r.SportId,
            r.Sport?.Name ?? string.Empty,
            r.BranchId,
            r.Branch?.Name ?? string.Empty,
            r.StartDate,
            r.GroupType,
            r.TrainingDays,
            r.PaymentTypeId,
            r.DiscountCode,
            r.Status,
            r.RequestedByUserId,
            nameLookup.GetValueOrDefault(r.RequestedByUserId, string.Empty),
            r.RequestedAt,
            r.ReviewedByUserId,
            r.ReviewedByUserId.HasValue ? nameLookup.GetValueOrDefault(r.ReviewedByUserId.Value) : null,
            r.ReviewedAt,
            r.RejectionReason,
            r.CreatedSubscriptionDetailsId);
    }
}
