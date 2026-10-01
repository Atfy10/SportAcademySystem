using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Domain.Entities.Events;

namespace SportAcademy.Application.Interfaces
{
    public interface IEventCustomerRepository : IBaseRepository<EventCustomer, int>
    {
        // phoneE164 must already be normalized (IPhoneNumberNormalizer) - it's matched exactly.
        Task<EventCustomer?> GetByPhoneAsync(string phoneE164, CancellationToken ct = default);

        Task<bool> IsPhoneTakenAsync(string phoneE164, int? excludeId, CancellationToken ct = default);

        Task<bool> HasEventsAsync(int customerId, CancellationToken ct = default);

        Task<(List<EventCustomerDto> Items, int TotalCount)> GetPagedAsync(
            PageRequest page, string? term, int? nationalityCategoryId, bool? isActive, string lang,
            CancellationToken ct = default);

        Task<EventCustomerDto?> GetSummaryAsync(int id, string lang, CancellationToken ct = default);
    }
}
