using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Application.Interfaces
{
    public interface IImpersonationGrantRepository
    {
        Task AddAsync(TenantImpersonationGrant grant, CancellationToken ct = default);

        /// <summary>Raw entity lookup by id - used by ImpersonationGuardMiddleware (which needs
        /// ExpiresAt/EndedAt to decide whether a request may proceed) and
        /// EndImpersonationCommandHandler.</summary>
        Task<TenantImpersonationGrant?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task UpdateAsync(TenantImpersonationGrant grant, CancellationToken ct = default);

        Task<PagedData<TenantImpersonationGrantDto>> GetPagedForTenantAsync(
            Guid tenantId, PageRequest page, CancellationToken ct = default);
    }
}
