using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.EndImpersonation;

public class EndImpersonationCommandHandler : IRequestHandler<EndImpersonationCommand, Result>
{
    private readonly IImpersonationGrantRepository _grantRepository;
    private readonly string _operation = OperationType.Update.ToString();

    public EndImpersonationCommandHandler(IImpersonationGrantRepository grantRepository)
    {
        _grantRepository = grantRepository;
    }

    public async Task<Result> Handle(EndImpersonationCommand request, CancellationToken ct)
    {
        var grant = await _grantRepository.GetByIdAsync(request.GrantId, ct);
        if (grant is null)
            return Result.Failure(_operation, "Impersonation session not found.", 404);

        request.ResolvedTenantId = grant.TenantId;
        request.ResolvedBeforeState = new { grant.EndedAt, grant.EndedReason };

        if (grant.EndedAt is not null)
            return Result.Failure(_operation, "This impersonation session has already ended.", 400);

        grant.EndedAt = DateTime.UtcNow;
        grant.EndedReason = "Manual";
        await _grantRepository.UpdateAsync(grant, ct);

        return Result.Success(_operation, "Impersonation session ended.");
    }
}
