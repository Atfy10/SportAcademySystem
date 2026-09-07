using System.Text.RegularExpressions;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Implementations;

/// <summary>
/// Wraps the real email provider: appends every outgoing email's link to a local text file
/// before attempting the real send, so an invitation/password-reset link is still recoverable if
/// the provider is unreachable, out of credits, misconfigured, or down - never as a substitute
/// for actually sending, always in addition to it. Wired up in every environment (see
/// Program.cs) - SendOwnerPasswordResetLinkCommandHandler and InvitationCreatedHandler both rely
/// on this file already holding the link before they swallow a send failure instead of
/// reporting it.
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
            // Defensive, not load-bearing: the container image/volume already creates this
            // directory (see the Dockerfile and Storage:LogsPath), but a misconfigured LogsPath
            // pointing somewhere new shouldn't turn every send into a startup-blocking crash.
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var fileExisted = File.Exists(_filePath);
            await File.AppendAllTextAsync(_filePath, line, ct);

            // Each line is a live, unexpired credential-granting URL until its token expires or
            // is used - lock the file to the running account only, the first time it's created.
            // Best-effort: Windows local dev has no POSIX mode bits, and a failure here must
            // never block the send this decorator exists to guarantee gets logged.
            if (!fileExisted && OperatingSystem.IsLinux())
            {
                try
                {
                    File.SetUnixFileMode(_filePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }
                catch
                {
                    // Best-effort only - see comment above.
                }
            }
        }
        finally
        {
            FileLock.Release();
        }
    }
}
