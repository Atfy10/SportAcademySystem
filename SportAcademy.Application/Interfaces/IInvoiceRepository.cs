using SportAcademy.Application.Common.Pagination;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Application.Interfaces
{
    public interface IInvoiceRepository : IBaseRepository<Invoice, int>
    {
        Task<Invoice?> GetWithLinesAndAllocationsAsync(int invoiceId, CancellationToken ct = default);
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default);
        Task<Invoice?> GetBySubscriptionDetailsIdAsync(int subscriptionDetailsId, CancellationToken ct = default);
        Task<List<Invoice>> GetByIdsWithLinesAsync(IEnumerable<int> ids, CancellationToken ct = default);

        Task<(List<Invoice> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, int? branchId, string? status, CancellationToken ct = default);

        Task<(List<Invoice> Items, int TotalCount)> GetOutstandingAsync(
            PageRequest page, int? branchId, bool overdueOnly, CancellationToken ct = default);

        Task<(decimal TotalOutstanding, int InvoiceCount, int OverdueCount, decimal OverdueAmount)> GetOutstandingSummaryAsync(
            int? branchId, CancellationToken ct = default);
    }
}
