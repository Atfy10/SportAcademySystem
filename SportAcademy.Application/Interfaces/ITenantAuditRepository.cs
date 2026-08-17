using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Application.Interfaces
{
    public interface ITenantAuditRepository
    {
        Task AddAsync(TenantAuditEvent auditEvent, CancellationToken ct = default);
        Task<PagedData<TenantAuditEventDto>> GetPagedAsync(
            Guid? tenantId, string? eventType, DateTime? from, DateTime? to,
            PageRequest page, CancellationToken ct = default);
    }
}
