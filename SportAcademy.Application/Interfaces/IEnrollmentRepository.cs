using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IEnrollmentRepository : IBaseRepository<Enrollment, int>
    {
        Task<PagedData<EnrollmentsSportsDto>> GetAllEnrollmentsForAllSports(
            PageRequest page,
            DateTime? from,
            DateTime? to,
            CancellationToken ct = default);
        Task<EnrollmentsSportDto> GetAllEnrollmentsForSport(
            PageRequest page,
            DateTime? from,
            DateTime? to,
            int sportId,
            CancellationToken ct = default);
        Task<int> GetEnrollmentsCountForSports(
            DateTime? from,
            DateTime? to,
            CancellationToken ct = default);
        Task<int> GetEnrollmentsCountForSport(
            int sportId,
            DateTime? from,
            DateTime? to,
            CancellationToken ct = default);
        Task<int?> GetEnrollmentIdAsync(int traineeId, int traineeGroupId, CancellationToken ct = default);
        Task<int> GetActiveEnrollmentCountForGroupAsync(int traineeGroupId, CancellationToken ct = default);
        Task<List<Enrollment>> GetActiveEnrollmentsForGroupAsync(int traineeGroupId, CancellationToken ct = default);
        Task<List<EnrollmentDetailDto>> GetAllDetailsByTraineeIdAsync(int traineeId, CancellationToken ct = default);
        /// <summary>
        /// The trainee's current enrollment for the given sport, if any - a trainee may have at
        /// most one (enforced at creation), but this is defensive against pre-existing data from
        /// before that rule existed, picking the most recently created one. Returns the tracked
        /// entity, not a DTO, since callers mutate and save it.
        /// </summary>
        Task<Enrollment?> GetCurrentEnrollmentForSportAsync(int traineeId, int sportId, CancellationToken ct = default);
        Task<PagedData<EnrollmentCardDto>> SearchAsync(string term, PageRequest page, string? status = null, string? paymentStatus = null, CancellationToken ct = default);
        Task<int> CountAllAsync(CancellationToken ct = default);
        Task<int> CountActiveAsync(CancellationToken ct = default);
        Task<int> CountPendingPaymentAsync(CancellationToken ct = default);
        Task<PagedData<EnrollmentCardDto>> GetAllAsync(PageRequest page, string? status = null, string? paymentStatus = null, CancellationToken ct = default);
        Task<EnrollmentDetailDto?> GetDetailByIdAsync(int id, CancellationToken ct = default);
    }
}
