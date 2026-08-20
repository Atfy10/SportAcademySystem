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

        /// Atomically revokes the token identified by <paramref name="tokenId"/> only if it is
        /// not already revoked, in a single conditional UPDATE. Returns false if another
        /// concurrent call already revoked it first - callers must treat that as "lost the
        /// race" rather than proceeding to rotate the same token twice.
        Task<bool> TryRevokeAsync(int tokenId, DateTime revokedAt, CancellationToken ct = default);
    }
}
