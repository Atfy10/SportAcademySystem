using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SportAcademy.Infrastructure.Implementations
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private const int RefreshTokenExpiryDays = 7;
        private const int GracePeriodMinutes = 10;

        public JwtTokenService(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
        {
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public string GenerateJwtToken(AppUser appUser, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, appUser.Id.ToString()),
                new(ClaimTypes.NameIdentifier, appUser.Id.ToString()),
                new("tenant_id", appUser.TenantId.ToString()),
                //new("tenant_code", appUser.Tenant.Code),
                new(JwtRegisteredClaimNames.UniqueName, appUser.UserName!),
                new(JwtRegisteredClaimNames.Email, appUser.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = _configuration["Jwt:Key"];
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha512);

            var expireMinutes = int.TryParse(_configuration["Jwt:ExpireMinutes"], out var parsed) ? parsed : 30;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                SigningCredentials = signingCredentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                IssuedAt = DateTime.UtcNow,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public string HashToken(string token)
        {
            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashedBytes);
        }

        public RefreshTokenResult? ValidateAndRefreshToken(string plainRefreshToken)
        {
            return ValidateAndRefreshTokenAsync(plainRefreshToken).GetAwaiter().GetResult();
        }

        public async Task<RefreshTokenResult?> ValidateAndRefreshTokenAsync(string plainRefreshToken, CancellationToken ct = default)
        {
            var tokenHash = HashToken(plainRefreshToken);

            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

            if (storedToken is null)
                return null;

            if (storedToken.IsRevoked)
                return null;

            var now = DateTime.UtcNow;
            var gracePeriodExpiry = storedToken.ExpiresAt.AddMinutes(GracePeriodMinutes);

            if (gracePeriodExpiry < now)
                return null;

            if (storedToken.User is null)
                return null;

            var roles = storedToken.User.UserRoles.Select(r => r.Role.Name ?? "").ToArray();
            var newAccessToken = GenerateJwtToken(storedToken.User, roles);
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshTokenHash = HashToken(newRefreshToken);

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = now;
            await _refreshTokenRepository.UpdateAsync(storedToken, ct);

            var newToken = new RefreshToken
            {
                TokenHash = newRefreshTokenHash,
                UserId = storedToken.UserId,
                ExpiresAt = now.AddDays(RefreshTokenExpiryDays),
                CreatedAt = now,
                IsRevoked = false
            };
            newToken = await _refreshTokenRepository.AddAsync(newToken, ct);

            storedToken.ReplacedByTokenId = newToken.Id;
            await _refreshTokenRepository.UpdateAsync(storedToken, ct);

            return new RefreshTokenResult(newAccessToken, newRefreshToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default)
        {
            return await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            await _refreshTokenRepository.UpdateAsync(token, ct);
        }
    }
}
