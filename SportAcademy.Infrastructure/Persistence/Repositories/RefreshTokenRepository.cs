using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> AddAsync(RefreshToken token, CancellationToken ct = default)
        {
            await _context.RefreshTokens.AddAsync(token, ct);
            await _context.SaveChangesAsync(ct);
            return token;
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        {
            // IgnoreQueryFilters(): this runs from the anonymous /auth/refresh endpoint, where
            // no tenant claim exists yet - ITenantIdProvider.TenantId is null at this point,
            // which makes the global tenant query filter (e.TenantId == CurrentTenantId)
            // silently null out the Included AppUser navigation for every row, since a null
            // CurrentTenantId can never match a real TenantId. Identity has to be resolved from
            // the token itself before any tenant context exists, so the filter must be
            // bypassed here - see the explicit IsDeleted/IsBanned checks in
            // JwtTokenService.ValidateAndRefreshTokenAsync, which take over what the
            // (also-bypassed) soft-delete filter would otherwise have enforced.
            return await _context.RefreshTokens
                .IgnoreQueryFilters()
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);
        }

        public async Task<RefreshToken?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            // See GetByTokenHashAsync above - same anonymous-context tenant-filter bypass.
            return await _context.RefreshTokens
                .IgnoreQueryFilters()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Id == id, ct);
        }

        public async Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
        {
            _context.RefreshTokens.Attach(token);
            _context.Entry(token).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> TryRevokeAsync(int tokenId, DateTime revokedAt, CancellationToken ct = default)
        {
            var rowsAffected = await _context.RefreshTokens
                .Where(rt => rt.Id == tokenId && !rt.IsRevoked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(rt => rt.IsRevoked, true)
                    .SetProperty(rt => rt.RevokedAt, revokedAt), ct);

            return rowsAffected > 0;
        }
    }
}
