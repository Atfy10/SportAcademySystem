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
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken ct = default);
        Task RevokeAllUserTokensAsync(string userId, CancellationToken ct = default);
    }
}
