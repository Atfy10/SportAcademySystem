namespace SportAcademy.Domain.Contract;

public interface IInvitationTokenService
{
    string GenerateRawToken();
    string HashToken(string rawToken);
}
