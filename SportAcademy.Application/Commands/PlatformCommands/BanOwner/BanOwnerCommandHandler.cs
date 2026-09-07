using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.BanOwner;

// Distinct from ToggleUserActiveCommand: that one is tenant-scoped (an Admin/Owner banning a
// user in their OWN tenant, via the normal tenant-filtered GetByIdAsync). This is the
// SuperAdmin/platform-console equivalent - the target Owner belongs to a DIFFERENT tenant than
// the caller (System), so it must resolve cross-tenant and is explicitly restricted to users
// holding the Owner role (see GetOwnerByIdAsync) rather than any AppUser by id.
public class BanOwnerCommandHandler : IRequestHandler<BanOwnerCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly string _operation = OperationType.Update.ToString();

    public BanOwnerCommandHandler(
        IUserRepository userRepository,
        ITenantIdProvider tenantIdProvider,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _tenantIdProvider = tenantIdProvider;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<bool>> Handle(BanOwnerCommand request, CancellationToken ct)
    {
        var owner = await _userRepository.GetOwnerByIdAsync(request.OwnerUserId, ct);
        if (owner is null)
            return Result<bool>.Failure(_operation, "Owner not found.", 404);

        // PlatformAuditBehavior reads this back once Handle() returns - this command targets an
        // owner, not a tenant, so it has no TenantId of its own to attribute the event to.
        request.ResolvedTenantId = owner.TenantId;
        request.ResolvedBeforeState = new { owner.IsBanned };

        owner.IsBanned = request.Banned;

        // TenantSaveChangesInterceptor rejects modifying an ITenantScoped entity (AppUser
        // included) whose TenantId doesn't match the ambient tenant - which, for a SuperAdmin,
        // is the System tenant, not the owner's real tenant. Align the ambient tenant to the
        // entity's own tenant for this write, same technique AppDataSeeder uses for its
        // cross-tenant inserts. Scoped via Impersonate() (not a bare SetTenantId) so the ambient
        // tenant is restored once this write is done - otherwise everything later in the same
        // request (including platform audit logging) would run under the owner's tenant instead
        // of the caller's real one.
        using (_tenantIdProvider.Impersonate(owner.TenantId))
        {
            await _userRepository.UpdateAsync(owner, ct);
        }

        // Only on the way to banned: un-banning shouldn't touch anything here, the owner just
        // logs back in normally. A live session must not survive a ban until its access token
        // happens to expire on its own (F-02) - JwtTokenService already refuses to refresh a
        // banned user's token, but this closes the gap immediately rather than waiting for the
        // next refresh attempt to fail.
        if (request.Banned)
            await _refreshTokenRepository.RevokeAllUserTokensAsync(owner.Id, ct);

        return Result<bool>.Success(owner.IsBanned, _operation);
    }
}
