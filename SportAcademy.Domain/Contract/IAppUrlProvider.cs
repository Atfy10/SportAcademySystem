namespace SportAcademy.Domain.Contract;

public interface IAppUrlProvider
{
    string BaseUrl { get; }
    string InvitationUrl(string slug, string rawToken);
    string PasswordResetUrl(Guid userId, string token);
    string EmailVerificationUrl(string token);
}
