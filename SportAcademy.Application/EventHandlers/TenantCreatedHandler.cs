using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

public sealed class TenantCreatedHandler : INotificationHandler<TenantCreatedEvent>
{
    private readonly IInvitationTokenService _tokenService;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly ILogger<TenantCreatedHandler> _logger;

    public TenantCreatedHandler(
        IInvitationTokenService tokenService,
        IInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ITenantIdProvider tenantIdProvider,
        ILogger<TenantCreatedHandler> logger)
    {
        _tokenService = tokenService;
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _tenantIdProvider = tenantIdProvider;
        _logger = logger;
    }

    public async Task Handle(TenantCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "TenantCreatedEvent: Tenant {TenantId} provisioned. Provisioning Owner invitation for {Email}.",
            notification.TenantId, notification.OwnerEmail);

        var rawToken = _tokenService.GenerateRawToken();
        var tokenHash = _tokenService.HashToken(rawToken);

        var invitation = InvitationMapper.ToEntity(
            notification.TenantId,
            notification.OwnerEmail,
            Guid.Empty,
            tokenHash,
            DateTime.UtcNow.AddDays(7));

        // The caller (Super Admin, provisioning a brand-new tenant) belongs to the SYSTEM
        // tenant, not this one. Without switching context, TenantSaveChangesInterceptor would
        // silently stamp this Invitation with the caller's own TenantId instead of the new
        // tenant's, which later makes AcceptInvitationCommandHandler resolve the wrong tenant.
        var previousTenantId = _tenantIdProvider.TenantId;
        _tenantIdProvider.SetTenantId(notification.TenantId);
        try
        {
            await _invitationRepository.AddAsync(invitation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _tenantIdProvider.SetTenantId(previousTenantId);
        }

        await _mediator.Publish(
            new InvitationCreatedEvent(
                invitation.Id,
                rawToken,
                notification.TenantSlug,
                notification.OwnerEmail),
            cancellationToken);

        _logger.LogInformation(
            "Owner invitation {InvitationId} provisioned for tenant {TenantId}.",
            invitation.Id, notification.TenantId);
    }
}
