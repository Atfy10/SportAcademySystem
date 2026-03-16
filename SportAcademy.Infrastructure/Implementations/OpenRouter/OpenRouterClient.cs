using Microsoft.Extensions.Configuration;
using SportAcademy.Application.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SportAcademy.Infrastructure.Implementations.OpenRouter;

public class OpenRouterClient : IOpenRouterClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenRouterClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenRouterSettings:ApiKey"]!;
        _model = configuration["OpenRouterSettings:Model"] ?? "google/gemma-3-12b-it:free";
    }

    public async Task<string> SendAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            "https://openrouter.ai/api/v1/chat/completions");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        httpRequest.Headers.Add("HTTP-Referer", "https://sportacademy.app");
        httpRequest.Headers.Add("X-Title", "SportAcademy Video Analysis");

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OpenRouter API error ({response.StatusCode}): {json}");
        }

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }
}
