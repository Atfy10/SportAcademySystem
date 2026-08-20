using Microsoft.AspNetCore.Identity;
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
        private readonly RoleManager<AppRole> _roleManager;
        private const int RefreshTokenExpiryDays = 7;
        private const int GracePeriodMinutes = 10;

        public JwtTokenService(
            IConfiguration configuration,
            IRefreshTokenRepository refreshTokenRepository,
            RoleManager<AppRole> roleManager)
        {
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _roleManager = roleManager;
        }

        public async Task<string> GenerateJwtToken(AppUser appUser, params string[] roles)
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

            var permissions = new HashSet<string>();
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

                // Permission claims are resolved from the role's own claims (seeded in
                // AppDataSeeder) at token-issue time, not stored on the user directly - this
                // keeps a single source of truth per role and avoids a DB round-trip per
                // authorization check later.
                var appRole = await _roleManager.FindByNameAsync(role);
                if (appRole is null) continue;
                var roleClaims = await _roleManager.GetClaimsAsync(appRole);
                foreach (var claim in roleClaims.Where(c => c.Type == "permission"))
                    permissions.Add(claim.Value);
            }

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
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
            {
                // Reuse of an already-rotated refresh token is a strong signal that the token
                // was stolen: an attacker who captured it before the legitimate rotation could
                // otherwise keep using the live child token forever. Revoke the whole family.
                await _refreshTokenRepository.RevokeAllUserTokensAsync(storedToken.UserId, ct);
                return null;
            }

            var now = DateTime.UtcNow;
            var gracePeriodExpiry = storedToken.ExpiresAt.AddMinutes(GracePeriodMinutes);

            if (gracePeriodExpiry < now)
                return null;

            if (storedToken.User is null)
                return null;

            // GetByTokenHashAsync bypasses query filters (see its comment) to resolve identity
            // before any tenant context exists, so the soft-delete filter that would normally
            // exclude a deleted user never runs here either - enforce both checks explicitly,
            // mirroring what LoginCommandHandler already enforces for the same user.
            if (storedToken.User.IsDeleted || storedToken.User.IsBanned)
                return null;

            // Atomically revoke only if still unrevoked. If a concurrent request already won
            // this exact rotation between the read above and here, this call returns false and
            // we bail out instead of both requests minting a "new" token from the same parent.
            var wonRotation = await _refreshTokenRepository.TryRevokeAsync(storedToken.Id, now, ct);
            if (!wonRotation)
                return null;

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = now;

            var roles = storedToken.User.UserRoles.Select(r => r.Role.Name ?? "").ToArray();
            var newAccessToken = await GenerateJwtToken(storedToken.User, roles);
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshTokenHash = HashToken(newRefreshToken);

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
