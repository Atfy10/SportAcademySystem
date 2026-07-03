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
    private readonly ILogger<TenantCreatedHandler> _logger;

    public TenantCreatedHandler(
        IInvitationTokenService tokenService,
        IInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<TenantCreatedHandler> logger)
    {
        _tokenService = tokenService;
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
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

        await _invitationRepository.AddAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
