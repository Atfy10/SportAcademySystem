using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IPaymentRepository : IBaseRepository<Payment, string>
    {
        Task<List<PaymentHistoryDto>> GetHistoryForTraineeAsync(int traineeId, CancellationToken cancellationToken = default);
        Task<Payment?> GetWithAllocationsAsync(string paymentNumber, CancellationToken ct = default);

        Task<(List<Payment> Items, int TotalCount)> GetPagedAsync(
            Common.Pagination.PageRequest page, int? branchId, int? paymentTypeId, string? status,
            DateTime? from, DateTime? to, CancellationToken ct = default);

        Task<List<(string GroupKey, decimal Gross, decimal Refunded, int Count)>> GetRevenueByMonthAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default);

        Task<List<(string GroupKey, decimal Gross, decimal Refunded, int Count)>> GetRevenueByBranchAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default);

        Task<List<(string PaymentTypeName, decimal Total, int Count)>> GetPaymentMethodBreakdownAsync(
            DateTime? from, DateTime? to, int? branchId, CancellationToken ct = default);
    }
}
