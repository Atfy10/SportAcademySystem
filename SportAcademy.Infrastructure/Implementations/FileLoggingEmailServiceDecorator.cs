using System.Text.RegularExpressions;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Implementations;

/// <summary>
/// Development-only decorator: appends every outgoing email's link to a local text file
/// before delegating to the real provider, so links are visible without checking an inbox.
/// Wired up only when the environment is Development (see Program.cs) - never used in production.
/// </summary>
public sealed class FileLoggingEmailServiceDecorator : IEmailService
{
    private static readonly SemaphoreSlim FileLock = new(1, 1);

    private readonly IEmailService _inner;
    private readonly string _filePath;

    public FileLoggingEmailServiceDecorator(IEmailService inner, string filePath)
    {
        _inner = inner;
        _filePath = filePath;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        await AppendLinkAsync(to, subject, htmlBody, ct);
        await _inner.SendAsync(to, subject, htmlBody, ct);
    }

    private async Task AppendLinkAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        var match = Regex.Match(htmlBody, "href=\"([^\"]+)\"");
        var link = match.Success ? match.Groups[1].Value : "(no link found in email body)";
        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} | {to} | {subject} | {link}{Environment.NewLine}";

        await FileLock.WaitAsync(ct);
        try
        {
            await File.AppendAllTextAsync(_filePath, line, ct);
        }
        finally
        {
            FileLock.Release();
        }
    }
}
