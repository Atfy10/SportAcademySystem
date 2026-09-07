using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    public interface ITenantAuditRepository
    {
        /// <summary>Inserts and immediately saves - for a standalone write with nothing else to
        /// batch with (PermissionAuthorizationHandler's Denied events).</summary>
        Task AddAsync(TenantAuditEvent auditEvent, CancellationToken ct = default);

        /// <summary>Stages the insert on the shared context without saving - for
        /// PlatformAuditBehavior, which commits it in the same SaveChanges/transaction as the
        /// handler it's auditing.</summary>
        Task AddWithoutSaveAsync(TenantAuditEvent auditEvent, CancellationToken ct = default);

        Task<PagedData<TenantAuditEventDto>> GetPagedAsync(
            Guid? tenantId, string? eventType, AuditOutcome? outcome, DateTime? from, DateTime? to,
            PageRequest page, CancellationToken ct = default);
    }
}
