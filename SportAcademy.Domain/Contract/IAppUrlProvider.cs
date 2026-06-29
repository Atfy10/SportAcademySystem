namespace SportAcademy.Domain.Contract;

public interface IAppUrlProvider
{
    string BaseUrl { get; }
    string InvitationUrl(string slug, string rawToken);
    string PasswordResetUrl(string token);
    string EmailVerificationUrl(string token);
}
