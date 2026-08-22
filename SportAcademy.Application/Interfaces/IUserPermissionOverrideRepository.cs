using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IUserPermissionOverrideRepository
    {
        Task<List<UserPermissionOverride>> GetForUserAsync(Guid userId, CancellationToken ct = default);

        Task AddRangeAsync(IEnumerable<UserPermissionOverride> overrides, CancellationToken ct = default);

        // Full replace: deletes every existing override for the user not present in
        // `overrides`, upserts the rest. Mirrors AssignRolesToUserCommand's replace-the-set
        // semantics for roles, and IUserRepository's previous remove-all-then-add behavior for
        // the permission claims this table now replaces.
        Task ReplaceForUserAsync(Guid userId, Guid tenantId,
            IReadOnlyCollection<UserPermissionOverride> overrides, CancellationToken ct = default);
    }
}
