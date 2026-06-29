using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly ApplicationDbContext _context;

    public InvitationRepository(ApplicationDbContext context) => _context = context;

    public Task<Invitation?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => _context.Set<Invitation>().FirstOrDefaultAsync(i => i.TokenHash == tokenHash, ct);

    public async Task AddAsync(Invitation invitation, CancellationToken ct = default)
        => await _context.Set<Invitation>().AddAsync(invitation, ct);
}
