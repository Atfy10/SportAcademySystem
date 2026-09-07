using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> AddAsync(RefreshToken token, CancellationToken ct = default);
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
        Task<RefreshToken?> GetByIdAsync(int id, CancellationToken ct = default);
        Task UpdateAsync(RefreshToken token, CancellationToken ct = default);
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default);

        /// Revokes every live refresh token for each of the given users in one bulk update.
        /// Used to kill existing sessions the moment an owner is banned or a tenant is
        /// suspended/archived/deactivated (F-02) - without this, a held refresh token would only
        /// stop working the next time it was actually presented (JwtTokenService rejects it by
        /// then), rather than being immediately and visibly revoked.
        Task RevokeAllTokensForUsersAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);

        /// Atomically revokes the token identified by <paramref name="tokenId"/> only if it is
        /// not already revoked, in a single conditional UPDATE. Returns false if another
        /// concurrent call already revoked it first - callers must treat that as "lost the
        /// race" rather than proceeding to rotate the same token twice.
        Task<bool> TryRevokeAsync(int tokenId, DateTime revokedAt, CancellationToken ct = default);
    }
}
