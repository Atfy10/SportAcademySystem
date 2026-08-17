using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class TenantAuditRepository : ITenantAuditRepository
    {
        private readonly ApplicationDbContext _context;

        public TenantAuditRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TenantAuditEvent auditEvent, CancellationToken ct = default)
        {
            await _context.Set<TenantAuditEvent>().AddAsync(auditEvent, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<PagedData<TenantAuditEventDto>> GetPagedAsync(
            Guid? tenantId, string? eventType, DateTime? from, DateTime? to,
            PageRequest page, CancellationToken ct = default)
        {
            var query = _context.Set<TenantAuditEvent>().AsNoTracking().AsQueryable();

            if (tenantId.HasValue)
                query = query.Where(e => e.TenantId == tenantId.Value);
            if (!string.IsNullOrWhiteSpace(eventType))
                query = query.Where(e => e.EventType == eventType);
            if (from.HasValue)
                query = query.Where(e => e.PerformedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.PerformedAt <= to.Value);

            query = query.OrderByDescending(e => e.PerformedAt);

            var totalCount = await query.CountAsync(ct);
            var pageEntities = await query
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            var items = pageEntities.Select(e => new TenantAuditEventDto(
                e.Id.ToString(),
                e.TenantId,
                e.EventType,
                e.Description,
                e.PerformedAt,
                e.PerformedBy
            )).ToList();

            return new PagedData<TenantAuditEventDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize,
            };
        }
    }
}
