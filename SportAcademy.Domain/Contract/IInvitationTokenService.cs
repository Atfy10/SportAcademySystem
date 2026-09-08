namespace SportAcademy.Domain.Contract;

public interface IInvitationTokenService
{
    string GenerateRawToken();
    string HashToken(string rawToken);

    /// <summary>Generates a random numeric email-verification code (e.g. "483920") - hash it with
    /// <see cref="HashToken"/> the same way the raw invitation token is hashed before storing.</summary>
    string GenerateNumericCode(int digits = 6);
}
