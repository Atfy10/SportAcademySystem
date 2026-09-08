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

    public string GenerateNumericCode(int digits = 6)
    {
        if (digits < 4)
            throw new ArgumentOutOfRangeException(nameof(digits), "A verification code shorter than 4 digits is too easy to brute-force.");

        // RandomNumberGenerator.GetInt32's upper bound is exclusive, so this is uniform over
        // every value with exactly `digits` digits, zero-padded (e.g. "004821", never "4821").
        var max = (int)Math.Pow(10, digits);
        var value = RandomNumberGenerator.GetInt32(max);
        return value.ToString().PadLeft(digits, '0');
    }
}
