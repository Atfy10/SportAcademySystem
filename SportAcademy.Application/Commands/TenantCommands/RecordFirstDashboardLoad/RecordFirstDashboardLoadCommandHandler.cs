using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.Commands.TenantCommands.RecordFirstDashboardLoad;

// Sent from DashboardController after a successful GetDashboardSummaryQuery - the sole call site
// of that query, same "one clear choke point" reasoning LoginCommandHandler already relies on for
// FirstTenantLoginEvent. This is the real "tenant is up and running" milestone: unlike a first
// login (which can happen mid-onboarding, e.g. the Owner closes the wizard and logs back in
// later), this only fires once the onboarding wizard has actually finished
// (TenantProfile.IsSetupComplete) - see FirstDashboardLoadAt's doc comment on Tenant.
public class RecordFirstDashboardLoadCommandHandler : IRequestHandler<RecordFirstDashboardLoadCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly string _operation = OperationType.Update.ToString();

    public RecordFirstDashboardLoadCommandHandler(
        ITenantRepository tenantRepository, IUnitOfWork unitOfWork, IMediator mediator)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result> Handle(RecordFirstDashboardLoadCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);

        // Every one of these is a normal, frequent no-op path (every dashboard load after the
        // first one, a SuperAdmin's own System-tenant dashboard, a tenant still mid-onboarding-
        // wizard) - none of them are error conditions worth a non-200, since this command's only
        // caller (DashboardController) must never let a milestone-tracking failure break an
        // otherwise-successful dashboard load.
        if (tenant is null || tenant.FirstDashboardLoadAt is not null || tenant.Code == Tenant.SystemTenantCode)
            return Result.Success(_operation, "Nothing to record.");

        var profile = await _tenantRepository.GetProfileAsync(request.TenantId, ct);
        if (profile is null || !profile.IsSetupComplete)
            return Result.Success(_operation, "Onboarding not yet complete.");

        tenant.FirstDashboardLoadAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _mediator.Publish(new FirstTenantDashboardLoadEvent(tenant.Id, tenant.DisplayName, request.UserId), ct);

        return Result.Success(_operation, "First dashboard load recorded.");
    }
}
