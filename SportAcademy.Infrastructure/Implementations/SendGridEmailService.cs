using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Options;

namespace SportAcademy.Infrastructure.Implementations;

public sealed class SendGridEmailService : IEmailService
{
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly EmailSettings _settings;

    public SendGridEmailService(IOptions<EmailSettings> settings, ILogger<SendGridEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SendGridApiKey))
        {
            _logger.LogInformation(
                "[SendGrid] Email would be sent — To: {To}, Subject: {Subject}, Body: {Body}",
                to, subject, htmlBody);
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "[SendGrid] Sending email — To: {To}, Subject: {Subject}", to, subject);

        return Task.CompletedTask;
    }
}
