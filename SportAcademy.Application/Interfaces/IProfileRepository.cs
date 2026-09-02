using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IProfileRepository : IBaseRepository<Profile, string>
    {
        // Profile.AppUserId is a Guid, not a string - IBaseRepository<Profile, string>'s
        // GetByIdAsync(string) can't correctly look one up. Use this instead.
        Task<Profile?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default);
    }
}
