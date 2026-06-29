using Microsoft.Extensions.Options;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Options;

namespace SportAcademy.Infrastructure.Implementations;

public sealed class AppUrlProvider : IAppUrlProvider
{
    private readonly AppUrlSettings _settings;

    public AppUrlProvider(IOptions<AppUrlSettings> settings)
    {
        _settings = settings.Value;
    }

    public string BaseUrl => _settings.BaseUrl.TrimEnd('/');

    public string InvitationUrl(string slug, string rawToken)
        => $"{BaseUrl}/t/{slug}/invite/{rawToken}";

    public string PasswordResetUrl(string token)
        => $"{BaseUrl}/reset-password?token={token}";

    public string EmailVerificationUrl(string token)
        => $"{BaseUrl}/verify-email?token={token}";
}
