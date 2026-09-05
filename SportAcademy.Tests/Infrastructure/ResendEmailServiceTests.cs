using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SportAcademy.Infrastructure.Implementations;
using SportAcademy.Infrastructure.Options;

namespace SportAcademy.Tests.Infrastructure;

/// <summary>
/// Pins what actually goes on the wire to Resend. None of this is visible at compile time, and a
/// wrong field name or sender format fails only at runtime, against a live account.
/// </summary>
public class ResendEmailServiceTests
{
    private sealed class CapturingHandler(HttpStatusCode status = HttpStatusCode.OK, string body = "{\"id\":\"abc\"}")
        : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Request = request;
            RequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(status) { Content = new StringContent(body) };
        }
    }

    private static ResendEmailService Build(CapturingHandler handler, string apiKey = "re_test_key")
        => new(
            new HttpClient(handler),
            Options.Create(new EmailSettings
            {
                Provider = "Resend",
                ResendApiKey = apiKey,
                FromEmail = "noreply@auraacademys.com",
                FromName = "AURA Academy",
            }),
            NullLogger<ResendEmailService>.Instance);

    [Fact]
    public async Task Sends_TheExpectedRequest()
    {
        var handler = new CapturingHandler();

        await Build(handler).SendAsync("trainee@example.com", "Welcome", "<p>Hi</p>");

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://api.resend.com/emails", handler.Request.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.Request.Headers.Authorization!.Scheme);
        Assert.Equal("re_test_key", handler.Request.Headers.Authorization.Parameter);

        using var body = JsonDocument.Parse(handler.RequestBody!);
        var root = body.RootElement;

        // Resend takes the sender as one "Name <address>" string, not separate fields the way
        // SendGrid does - getting this wrong is a 422 at send time.
        Assert.Equal("AURA Academy <noreply@auraacademys.com>", root.GetProperty("from").GetString());
        Assert.Equal("trainee@example.com", root.GetProperty("to")[0].GetString());
        Assert.Equal("Welcome", root.GetProperty("subject").GetString());
        Assert.Equal("<p>Hi</p>", root.GetProperty("html").GetString());
    }

    [Fact]
    public async Task WithoutAnApiKey_DoesNothingRatherThanFailing()
    {
        // Matches SendGridEmailService: an unconfigured local environment shouldn't turn every
        // invitation into an error.
        var handler = new CapturingHandler();

        await Build(handler, apiKey: "").SendAsync("trainee@example.com", "Welcome", "<p>Hi</p>");

        Assert.Null(handler.Request);
    }

    [Fact]
    public async Task ARejectionThrows_SoTheCallerCanLogIt()
    {
        var handler = new CapturingHandler(HttpStatusCode.Forbidden, "{\"message\":\"domain not verified\"}");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Build(handler).SendAsync("trainee@example.com", "Welcome", "<p>Hi</p>"));

        Assert.Contains("403", ex.Message);
    }
}
