using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Commands.AuthCommands.CreateInvitation;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantDetailResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IUserContextService _userContext;
    private readonly ILogger<CreateTenantCommandHandler> _logger;
    private readonly string _operation = OperationType.Add.ToString();

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IBaseRepository<SubscriptionPlan, int> planRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IUserContextService userContext,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<Result<TenantDetailResponse>> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        var slugUnique = await _tenantRepository.IsSlugUniqueAsync(request.Slug, null, ct);
        if (!slugUnique)
            return Result<TenantDetailResponse>.Failure(_operation, "Slug is already in use.", 400);

        var codeUnique = await _tenantRepository.IsCodeUniqueAsync(request.Code, null, ct);
        if (!codeUnique)
            return Result<TenantDetailResponse>.Failure(_operation, "Code is already in use.", 400);

        var plan = await _planRepository.GetByIdAsync(request.SubscriptionPlanId, ct);
        if (plan is null)
            return Result<TenantDetailResponse>.Failure(_operation, "Subscription plan not found.", 404);

        var now = DateTime.UtcNow;

        // A new tenant should start with whatever features its chosen plan includes, not with
        // everything disabled - otherwise SuperAdmin has to manually toggle every single
        // feature on right after creating the tenant, and the tenant is unusable until then.
        var planFeatureIds = await _tenantRepository.GetPlanFeaturesAsync(plan.Id, ct);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            DisplayName = request.DisplayName,
            Slug = request.Slug,
            Code = request.Code.ToUpper(),
            Email = request.Email,
            Status = TenantStatus.PendingSetup,
            CreatedAt = now,
            Profile = new TenantProfile
            {
                OrganizationName = request.DisplayName,
                Phone = request.Phone,
                Address = request.Address
            },
            Settings = new TenantSettings
            {
                TimeZone = request.TimeZone ?? "UTC",
                Language = request.Language ?? "en",
                DateFormat = "dd/MM/yyyy",
                TimeFormat = "HH:mm",
                Currency = request.Currency ?? "USD"
            },
            Subscription = new TenantSubscription
            {
                StartsAt = now,
                EndsAt = now.AddYears(1),
                IsTrial = true,
                AutoRenew = true,
                SubscriptionPlanId = plan.Id
            },
            Features = planFeatureIds.Select(featureId => new TenantFeature
            {
                FeatureId = featureId,
                IsEnabled = true,
                EnabledAt = now,
                EnabledBy = "System"
            }).ToList()
        };

        await _tenantRepository.AddAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // PlatformAuditBehavior reads this back once Handle() returns - the tenant doesn't
        // exist (and so has no id to attribute an audit event to) until this line runs.
        request.ResolvedTenantId = tenant.Id;

        // Reuses the exact same path a SuperAdmin re-inviting a stuck PendingSetup tenant's Owner
        // already goes through (CreateInvitationCommandHandler's Role==null branch), rather than
        // a bespoke tenant-creation-only invitation path that duplicated its rules. Attributed to
        // whichever SuperAdmin is creating this tenant (not Guid.Empty) - InvitationAcceptedHandler
        // can then actually notify them when the Owner accepts, the same as any other invite.
        string? ownerInviteUrl = null;
        var callerId = _userContext.UserId;
        if (callerId is { } invitedByUserId)
        {
            var invitationResult = await _mediator.Send(
                new CreateInvitationCommand(tenant.Id, request.OwnerEmail, invitedByUserId), ct);

            // A failure here must never undo a tenant that was already created successfully -
            // the tenant is left in PendingSetup with no invitation yet, recoverable later via
            // the existing "Resend Invitation" action once the underlying problem is fixed.
            if (invitationResult.IsSuccess)
                ownerInviteUrl = invitationResult.Data!.InviteUrl;
            else
                _logger.LogError(
                    "Tenant {TenantId} was created but its Owner invitation failed: {Message}",
                    tenant.Id, invitationResult.Message);
        }
        else
        {
            _logger.LogError(
                "Tenant {TenantId} was created without an authenticated caller in context - no Owner invitation could be attributed and none was created.",
                tenant.Id);
        }

        var detail = tenant.ToDetailResponse() with { OwnerInviteUrl = ownerInviteUrl };
        return Result<TenantDetailResponse>.Success(detail, _operation, "Tenant created successfully.");
    }
}
