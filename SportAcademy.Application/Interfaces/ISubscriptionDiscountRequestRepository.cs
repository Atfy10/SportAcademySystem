using SportAcademy.Application.Common.Pagination;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    public interface ISubscriptionDiscountRequestRepository : IBaseRepository<SubscriptionDiscountRequest, int>
    {
        Task<(List<SubscriptionDiscountRequest> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, SubscriptionDiscountRequestStatus? status, int? branchId,
            CancellationToken cancellationToken = default);

        Task<SubscriptionDiscountRequest?> GetByIdWithIncludesAsync(int id, CancellationToken cancellationToken = default);
    }
}
