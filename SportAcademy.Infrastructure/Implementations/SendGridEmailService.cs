using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Options;

namespace SportAcademy.Infrastructure.Implementations;

public sealed class SendGridEmailService : IEmailService
{
    private const string SendEndpoint = "https://api.sendgrid.com/v3/mail/send";

    private readonly HttpClient _httpClient;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly EmailSettings _settings;

    public SendGridEmailService(HttpClient httpClient, IOptions<EmailSettings> settings, ILogger<SendGridEmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SendGridApiKey))
        {
            // No provider configured (local/dev) - don't attempt a call that can only fail.
            // This is the one case where "not sending" is the correct, expected behavior.
            _logger.LogInformation(
                "[SendGrid] No API key configured - email NOT sent. To: {To}, Subject: {Subject}",
                to, subject);
            return;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, SendEndpoint)
        {
            Content = JsonContent.Create(new SendGridMailRequest
            {
                Personalizations = [new SendGridPersonalization { To = [new SendGridAddress { Email = to }] }],
                From = new SendGridAddress { Email = _settings.FromEmail, Name = _settings.FromName },
                Subject = subject,
                Content = [new SendGridContent { Type = "text/html", Value = htmlBody }],
            }),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SendGridApiKey);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "[SendGrid] Request failed for {To}", to);
            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "[SendGrid] Send failed for {To}: {StatusCode} {Body}",
                to, (int)response.StatusCode, body);
            throw new InvalidOperationException(
                $"SendGrid rejected the email to {to}: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        _logger.LogInformation("[SendGrid] Email accepted for delivery to {To}", to);
    }

    private sealed class SendGridMailRequest
    {
        [JsonPropertyName("personalizations")]
        public required List<SendGridPersonalization> Personalizations { get; init; }

        [JsonPropertyName("from")]
        public required SendGridAddress From { get; init; }

        [JsonPropertyName("subject")]
        public required string Subject { get; init; }

        [JsonPropertyName("content")]
        public required List<SendGridContent> Content { get; init; }
    }

    private sealed class SendGridPersonalization
    {
        [JsonPropertyName("to")]
        public required List<SendGridAddress> To { get; init; }
    }

    private sealed class SendGridAddress
    {
        [JsonPropertyName("email")]
        public required string Email { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    private sealed class SendGridContent
    {
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("value")]
        public required string Value { get; init; }
    }
}
