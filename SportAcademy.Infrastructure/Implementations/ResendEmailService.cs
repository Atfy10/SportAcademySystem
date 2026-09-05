using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Options;

namespace SportAcademy.Infrastructure.Implementations;

/// <summary>
/// Resend's REST API, called directly rather than through their SDK - same approach as
/// SendGridEmailService, and for the same reason: one POST with an API key needs no dependency.
/// Which of the two is used is decided by Email:Provider (see Program.cs); the two are
/// interchangeable because everything above IEmailService is provider-agnostic.
/// </summary>
public sealed class ResendEmailService : IEmailService
{
    private const string SendEndpoint = "https://api.resend.com/emails";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly EmailSettings _settings;

    public ResendEmailService(HttpClient httpClient, IOptions<EmailSettings> settings, ILogger<ResendEmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ResendApiKey))
        {
            // No provider configured (local/dev) - don't attempt a call that can only fail.
            // This is the one case where "not sending" is the correct, expected behavior.
            _logger.LogInformation(
                "[Resend] No API key configured - email NOT sent. To: {To}, Subject: {Subject}",
                to, subject);
            return;
        }

        // Resend takes the sender as a single field, in "Name <address>" form. The address must
        // belong to a domain verified on the account, or the send is rejected.
        var from = string.IsNullOrWhiteSpace(_settings.FromName)
            ? _settings.FromEmail
            : $"{_settings.FromName} <{_settings.FromEmail}>";

        var request = new HttpRequestMessage(HttpMethod.Post, SendEndpoint)
        {
            Content = JsonContent.Create(new ResendMailRequest
            {
                From = from,
                To = [to],
                Subject = subject,
                Html = htmlBody,
            }),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ResendApiKey);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "[Resend] Request failed for {To}", to);
            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "[Resend] Send failed for {To}: {StatusCode} {Body}",
                to, (int)response.StatusCode, body);
            throw new InvalidOperationException(
                $"Resend rejected the email to {to}: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        _logger.LogInformation("[Resend] Email accepted for delivery to {To}", to);
    }

    private sealed class ResendMailRequest
    {
        [JsonPropertyName("from")]
        public required string From { get; init; }

        [JsonPropertyName("to")]
        public required List<string> To { get; init; }

        [JsonPropertyName("subject")]
        public required string Subject { get; init; }

        [JsonPropertyName("html")]
        public required string Html { get; init; }
    }
}
