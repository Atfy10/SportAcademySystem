namespace SportAcademy.Application.Interfaces;

public interface IOpenRouterClient
{
    Task<string> SendAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken);
}
