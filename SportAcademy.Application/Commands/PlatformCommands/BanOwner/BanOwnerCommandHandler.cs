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
    private readonly string _operation = OperationType.Update.ToString();

    public BanOwnerCommandHandler(IUserRepository userRepository, ITenantIdProvider tenantIdProvider)
    {
        _userRepository = userRepository;
        _tenantIdProvider = tenantIdProvider;
    }

    public async Task<Result<bool>> Handle(BanOwnerCommand request, CancellationToken ct)
    {
        var owner = await _userRepository.GetOwnerByIdAsync(request.OwnerUserId, ct);
        if (owner is null)
            return Result<bool>.Failure(_operation, "Owner not found.", 404);

        owner.IsBanned = request.Banned;

        // TenantSaveChangesInterceptor rejects modifying an ITenantScoped entity (AppUser
        // included) whose TenantId doesn't match the ambient tenant - which, for a SuperAdmin,
        // is the System tenant, not the owner's real tenant. Align the ambient tenant to the
        // entity's own tenant for this write, same technique AppDataSeeder uses for its
        // cross-tenant inserts.
        _tenantIdProvider.SetTenantId(owner.TenantId);
        await _userRepository.UpdateAsync(owner, ct);

        return Result<bool>.Success(owner.IsBanned, _operation);
    }
}
