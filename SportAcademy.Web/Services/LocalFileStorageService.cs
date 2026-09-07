using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Services;

// Local-disk implementation for a single-instance deployment (no shared/distributed file store
// configured anywhere in this app - see appsettings.json). Everything lives under
// {UploadsPathResolver root}/{tenantId:N}/{category folder}, served back out by UseStaticFiles()
// in Program.cs (mounted at that same resolved root) - TenantFileAccessGuardMiddleware
// (registered just before it) reads the tenant id segment out of the URL to 403 a
// suspended/archived tenant's files, the same way every other tenant-scoped request is already
// gated.
public class LocalFileStorageService : IFileStorageService
{
    private static readonly Dictionary<ImageUploadCategory, string> CategoryFolders = new()
    {
        [ImageUploadCategory.Avatar] = "avatars",
        [ImageUploadCategory.Person] = "people",
        [ImageUploadCategory.TenantLogo] = "tenant-logos",
    };

    private const string UploadsUrlPrefix = "/uploads";

    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IWebHostEnvironment env,
        IConfiguration configuration,
        ITenantIdProvider tenantIdProvider,
        ILogger<LocalFileStorageService> logger)
    {
        _env = env;
        _configuration = configuration;
        _tenantIdProvider = tenantIdProvider;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(
        Stream content, string fileName, ImageUploadCategory category, CancellationToken ct = default)
    {
        var tenantId = _tenantIdProvider.TenantId
            ?? throw new InvalidOperationException("LocalFileStorageService invoked without a resolved tenant context.");
        var folder = CategoryFolders[category];
        var tenantSegment = tenantId.ToString("N");

        var uploadsRoot = UploadsPathResolver.Resolve(_configuration, _env);
        var targetDir = Path.Combine(uploadsRoot, tenantSegment, folder);
        Directory.CreateDirectory(targetDir);

        // Never trust the caller's filename beyond its extension - it becomes part of a
        // filesystem path (Path.GetFileName strips any directory traversal segments an
        // adversarial "../../evil.jpg" would otherwise carry) and the stored name is a fresh
        // GUID regardless.
        var extension = Path.GetExtension(Path.GetFileName(fileName));
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetDir, storedFileName);

        await using (var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, ct);
        }

        _logger.LogInformation("Saved uploaded image to {Path} ({Category})", fullPath, category);

        return $"{UploadsUrlPrefix}/{tenantSegment}/{folder}/{storedFileName}";
    }

    public void DeleteImage(string? relativeUrl)
    {
        // Only ever deletes a path this service itself generated (starts with /uploads/) - an
        // external URL (someone linking an image hosted elsewhere) or a null/empty value is
        // silently ignored rather than treated as an error the caller has to special-case.
        if (string.IsNullOrWhiteSpace(relativeUrl) || !relativeUrl.StartsWith(UploadsUrlPrefix, StringComparison.OrdinalIgnoreCase))
            return;

        var uploadsRoot = UploadsPathResolver.Resolve(_configuration, _env);

        // relativeSegments[0] is always "uploads" (guaranteed by the StartsWith check above) -
        // dropped here since uploadsRoot already points at the uploads directory itself, not its
        // parent.
        var relativeSegments = relativeUrl.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(1);
        var fullPath = Path.Combine([uploadsRoot, .. relativeSegments]);

        // Path.Combine can't be tricked by ".." segments arriving through relativeUrl into
        // escaping the uploads folder (they'd need to survive GetFullPath's normalization below
        // pointed back inside uploadsRoot), but the resolved path is verified to still be under
        // uploadsRoot before any delete happens, as a second, independent guard.
        var normalizedFullPath = Path.GetFullPath(fullPath);
        var normalizedWebRoot = Path.GetFullPath(uploadsRoot);
        if (!normalizedFullPath.StartsWith(normalizedWebRoot, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Refused to delete a path outside the web root: {RelativeUrl}", relativeUrl);
            return;
        }

        try
        {
            if (File.Exists(normalizedFullPath))
                File.Delete(normalizedFullPath);
        }
        catch (IOException ex)
        {
            // Best-effort cleanup - a locked/already-gone file must never fail the write
            // operation that's replacing it (e.g. re-uploading a new avatar over an old one).
            _logger.LogWarning(ex, "Failed to delete old image at {Path}", normalizedFullPath);
        }
    }
}
