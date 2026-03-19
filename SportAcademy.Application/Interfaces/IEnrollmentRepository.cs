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
        Task<PagedData<EnrollmentCardDto>> SearchAsync(string term, PageRequest page, CancellationToken ct = default);
        Task<int> CountAllAsync(CancellationToken ct = default);
        Task<int> CountActiveAsync(CancellationToken ct = default);
        Task<int> CountPendingPaymentAsync(CancellationToken ct = default);
        Task<PagedData<EnrollmentCardDto>> GetAllAsync(PageRequest page, CancellationToken ct = default);
        Task<EnrollmentDetailDto?> GetDetailByIdAsync(int id, CancellationToken ct = default);
    }
}
