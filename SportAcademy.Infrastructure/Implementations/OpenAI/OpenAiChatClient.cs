using Microsoft.Extensions.Configuration;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SportAcademy.Infrastructure.Implementations.OpenAi;

public class OpenAiChatClient : IOpenAiChatClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAiChatClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAiSettings:ApiKey"]!;
    }

    public async Task<string> SendAsync(
        IReadOnlyList<OpenAiMessage> messages,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            model = "gpt-4o-mini",
            messages = messages.Select(m => new { role = m.Role, content = m.Content })
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }
}
