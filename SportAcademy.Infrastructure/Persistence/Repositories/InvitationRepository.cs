using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly ApplicationDbContext _context;

    public InvitationRepository(ApplicationDbContext context) => _context = context;

    public Task<Invitation?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => _context.Set<Invitation>().IgnoreQueryFilters().FirstOrDefaultAsync(i => i.TokenHash == tokenHash, ct);

    public Task<List<Invitation>> GetPendingByTenantAndEmailAsync(Guid tenantId, string email, CancellationToken ct = default)
        => _context.Set<Invitation>()
            .IgnoreQueryFilters()
            .Where(i => i.TenantId == tenantId && i.Email == email && i.Status == InvitationStatus.Pending)
            .ToListAsync(ct);

    public async Task AddAsync(Invitation invitation, CancellationToken ct = default)
        => await _context.Set<Invitation>().AddAsync(invitation, ct);
}
