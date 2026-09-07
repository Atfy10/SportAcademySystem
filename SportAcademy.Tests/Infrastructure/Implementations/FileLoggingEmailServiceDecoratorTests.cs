using FluentAssertions;
using Moq;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Implementations;

namespace SportAcademy.Tests.Infrastructure.Implementations;

// This is the fallback SendOwnerPasswordResetLinkCommandHandler and InvitationCreatedHandler
// both rely on when they swallow a provider send failure instead of reporting it - see their own
// comments. Exercises the real filesystem under a throwaway temp path, same reasoning as
// LocalFileStorageServiceTests: the whole point of this class is disk I/O.
public class FileLoggingEmailServiceDecoratorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _filePath;
    private readonly Mock<IEmailService> _innerMock = new();

    public FileLoggingEmailServiceDecoratorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "sport-academy-tests-" + Guid.NewGuid().ToString("N"));
        _filePath = Path.Combine(_tempDir, "nested", "invitation-links.txt");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task SendAsync_WritesTheLinkBeforeDelegatingToTheRealProvider()
    {
        // Checked from inside the inner provider's own call, not after SendAsync returns - this
        // is what actually proves the write happens first, not just that it happens at all.
        string? contentAtSendTime = null;
        _innerMock
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => contentAtSendTime = File.Exists(_filePath) ? File.ReadAllText(_filePath) : null)
            .Returns(Task.CompletedTask);
        var decorator = new FileLoggingEmailServiceDecorator(_innerMock.Object, _filePath);

        await decorator.SendAsync("owner@example.com", "Reset your password", "<a href=\"https://app/reset/abc123\">Reset</a>");

        contentAtSendTime.Should().NotBeNull();
        contentAtSendTime.Should().Contain("https://app/reset/abc123");
    }

    [Fact]
    public async Task SendAsync_LinkAlreadyOnDisk_EvenWhenTheRealProviderThrows()
    {
        // The exact scenario this class exists for: a SendGrid/Resend outage must not lose the
        // link - SendOwnerPasswordResetLinkCommandHandler/InvitationCreatedHandler catch this
        // exception and rely on the file already being written by the time they do.
        _innerMock
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("provider down"));
        var decorator = new FileLoggingEmailServiceDecorator(_innerMock.Object, _filePath);

        var act = () => decorator.SendAsync("owner@example.com", "Invite", "<a href=\"https://app/invite/xyz\">Accept</a>");

        await act.Should().ThrowAsync<InvalidOperationException>();
        File.Exists(_filePath).Should().BeTrue();
        (await File.ReadAllTextAsync(_filePath)).Should().Contain("https://app/invite/xyz");
    }

    [Fact]
    public async Task SendAsync_CreatesTheTargetDirectoryIfItDoesNotExistYet()
    {
        // Storage:LogsPath pointing somewhere new (or a fresh container volume) must not turn
        // every send into a crash.
        Directory.Exists(Path.GetDirectoryName(_filePath)).Should().BeFalse();
        _innerMock
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var decorator = new FileLoggingEmailServiceDecorator(_innerMock.Object, _filePath);

        await decorator.SendAsync("owner@example.com", "Invite", "<a href=\"https://app/invite/xyz\">Accept</a>");

        File.Exists(_filePath).Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_NoHrefInBody_LogsAPlaceholderInsteadOfFailing()
    {
        _innerMock
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var decorator = new FileLoggingEmailServiceDecorator(_innerMock.Object, _filePath);

        await decorator.SendAsync("owner@example.com", "Plain notice", "<p>No link here.</p>");

        (await File.ReadAllTextAsync(_filePath)).Should().Contain("(no link found in email body)");
    }

    [Fact]
    public async Task SendAsync_CalledTwice_AppendsRatherThanOverwriting()
    {
        _innerMock
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var decorator = new FileLoggingEmailServiceDecorator(_innerMock.Object, _filePath);

        await decorator.SendAsync("first@example.com", "Invite", "<a href=\"https://app/invite/first\">Accept</a>");
        await decorator.SendAsync("second@example.com", "Invite", "<a href=\"https://app/invite/second\">Accept</a>");

        var content = await File.ReadAllTextAsync(_filePath);
        content.Should().Contain("https://app/invite/first");
        content.Should().Contain("https://app/invite/second");
    }
}
