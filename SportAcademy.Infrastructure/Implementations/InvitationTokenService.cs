using System.Security.Cryptography;
using System.Text;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Implementations;

public class InvitationTokenService : IInvitationTokenService
{
    public string GenerateRawToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public string HashToken(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
