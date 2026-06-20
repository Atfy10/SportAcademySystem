using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateJwtToken(AppUser appUser, params string[] roles);
        string GenerateRefreshToken();
        string HashToken(string token);
        Task<RefreshTokenResult?> ValidateAndRefreshTokenAsync(string plainRefreshToken, CancellationToken ct = default);
        Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default);
        Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    }

    public record RefreshTokenResult(string AccessToken, string RefreshToken);
}
