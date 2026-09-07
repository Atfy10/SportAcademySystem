using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    // Storage is local disk today (single-instance deployment - see wwwroot/uploads under the
    // Web project's implementation), but the write path (UploadImageCommandHandler) never
    // touches disk paths directly, so swapping in a cloud provider later only means writing a
    // new implementation of this interface, not touching any command/handler that uses it.
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves an uploaded image under the folder <paramref name="category"/> maps to and
        /// returns the relative URL (e.g. "/uploads/{tenantId}/avatars/{guid}.jpg") to store on
        /// the owning entity - never an absolute filesystem path. The tenant id segment is what
        /// TenantFileAccessGuardMiddleware reads to gate a suspended/archived tenant's files.
        /// </summary>
        Task<string> SaveImageAsync(
            Stream content, string fileName, ImageUploadCategory category, CancellationToken ct = default);

        /// <summary>
        /// Deletes a previously-saved file given the relative URL SaveImageAsync returned.
        /// A no-op (not an error) if the file doesn't exist, or if the value isn't a
        /// locally-stored path this service owns (e.g. it's null, or an external URL) - callers
        /// replacing an image call this for the old value without needing to know its origin.
        /// </summary>
        void DeleteImage(string? relativeUrl);
    }
}
