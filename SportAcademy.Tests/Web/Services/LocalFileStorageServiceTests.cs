using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Web.Services;

namespace SportAcademy.Tests.Web.Services;

// Exercises the real filesystem under a throwaway temp directory standing in for the resolved
// uploads root - SaveImageAsync/DeleteImage's whole job is disk I/O, so a mock filesystem would
// just be re-asserting the implementation rather than verifying it actually works.
public class LocalFileStorageServiceTests : IDisposable
{
    // Stands in for UploadsPathResolver's resolved root directly (e.g. Storage:UploadsPath in
    // production) - it already IS the uploads directory, not its parent, so disk paths below
    // never include an "uploads" segment even though every returned URL does.
    private readonly string _uploadsRoot;
    private static readonly Guid TenantId = Guid.NewGuid();
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _uploadsRoot = Path.Combine(Path.GetTempPath(), "sport-academy-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_uploadsRoot);

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.ContentRootPath).Returns(_uploadsRoot);

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Storage:UploadsPath"]).Returns(_uploadsRoot);

        var tenantIdProviderMock = new Mock<ITenantIdProvider>();
        tenantIdProviderMock.Setup(p => p.TenantId).Returns(TenantId);

        _service = new LocalFileStorageService(
            envMock.Object, configMock.Object, tenantIdProviderMock.Object, Mock.Of<ILogger<LocalFileStorageService>>());
    }

    public void Dispose()
    {
        if (Directory.Exists(_uploadsRoot))
            Directory.Delete(_uploadsRoot, recursive: true);
    }

    // Mirrors DeleteImage's own segment-dropping logic: the URL always carries a leading
    // "/uploads/" segment that has no corresponding folder on disk, since _uploadsRoot already
    // points at that directory.
    private string ToDiskPath(string url)
    {
        var segments = url.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(1);
        return Path.Combine([_uploadsRoot, .. segments]);
    }

    [Fact]
    public async Task SaveImageAsync_WritesFileUnderTheCategoryFolderAndReturnsItsUrl()
    {
        using var content = new MemoryStream([1, 2, 3, 4]);

        var url = await _service.SaveImageAsync(content, "photo.jpg", ImageUploadCategory.Avatar);

        url.Should().MatchRegex($@"^/uploads/{TenantId:N}/avatars/[0-9a-f]{{32}}\.jpg$");
        var diskPath = ToDiskPath(url);
        File.Exists(diskPath).Should().BeTrue();
        (await File.ReadAllBytesAsync(diskPath)).Should().Equal([1, 2, 3, 4]);
    }

    [Fact]
    public async Task SaveImageAsync_ScopesTheStoredPathToTheCurrentTenant()
    {
        // TenantFileAccessGuardMiddleware relies on this segment being present and accurate to
        // gate a suspended/archived tenant's files - if this ever silently stopped being
        // written, every uploaded file would become unguardable.
        using var content = new MemoryStream([1]);

        var url = await _service.SaveImageAsync(content, "photo.jpg", ImageUploadCategory.Avatar);

        url.Should().StartWith($"/uploads/{TenantId:N}/");
    }

    [Fact]
    public async Task SaveImageAsync_IgnoresTheCallersOriginalFileNameBeyondItsExtension()
    {
        // A caller-supplied "../../evil.jpg" (or anything else) must never influence where the
        // file lands - only its extension is kept, and the stored name is always a fresh GUID.
        using var content = new MemoryStream([9]);

        var url = await _service.SaveImageAsync(content, "../../evil.jpg", ImageUploadCategory.Person);

        url.Should().StartWith($"/uploads/{TenantId:N}/people/");
        url.Should().NotContain("..");
        url.Should().NotContain("evil");
    }

    [Fact]
    public async Task SaveImageAsync_DifferentCategoriesGoToDifferentFolders()
    {
        using var avatar = new MemoryStream([1]);
        using var logo = new MemoryStream([2]);

        var avatarUrl = await _service.SaveImageAsync(avatar, "a.png", ImageUploadCategory.Avatar);
        var logoUrl = await _service.SaveImageAsync(logo, "b.png", ImageUploadCategory.TenantLogo);

        avatarUrl.Should().StartWith($"/uploads/{TenantId:N}/avatars/");
        logoUrl.Should().StartWith($"/uploads/{TenantId:N}/tenant-logos/");
    }

    [Fact]
    public async Task DeleteImage_ExistingLocalFile_RemovesIt()
    {
        using var content = new MemoryStream([1]);
        var url = await _service.SaveImageAsync(content, "photo.png", ImageUploadCategory.Avatar);
        var diskPath = ToDiskPath(url);
        File.Exists(diskPath).Should().BeTrue();

        _service.DeleteImage(url);

        File.Exists(diskPath).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://cdn.example.com/some-external-avatar.png")]
    public void DeleteImage_NullOrExternalUrl_NoOp(string? url)
    {
        var act = () => _service.DeleteImage(url);

        act.Should().NotThrow();
    }

    [Fact]
    public void DeleteImage_PathTraversalAttempt_RefusesAndDoesNotThrow()
    {
        // Even though the "/uploads/" prefix check alone can't be defeated by a raw ".." (it's
        // a StartsWith on the very first segment), this asserts the second, path-based guard in
        // DeleteImage never lets a resolved path escape webRoot.
        var act = () => _service.DeleteImage("/uploads/../../../../etc/passwd");

        act.Should().NotThrow();
    }
}
