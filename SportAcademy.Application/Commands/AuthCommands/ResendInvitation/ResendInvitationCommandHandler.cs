using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.Commands.AuthCommands.ResendInvitation;

public class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand, Result<InvitationResponse>>
{
    private readonly IBaseRepository<Tenant, Guid> _tenantRepository;
    private readonly IInvitationTokenService _tokenService;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly string _operation = OperationType.Add.ToString();

    public ResendInvitationCommandHandler(
        IBaseRepository<Tenant, Guid> tenantRepository,
        IInvitationTokenService tokenService,
        IInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _tenantRepository = tenantRepository;
        _tokenService = tokenService;
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result<InvitationResponse>> Handle(ResendInvitationCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result<InvitationResponse>.Failure(_operation, "Tenant not found.", 404);

        var rawToken = _tokenService.GenerateRawToken();
        var tokenHash = _tokenService.HashToken(rawToken);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var newInvitation = InvitationMapper.ToEntity(
            request.TenantId,
            request.Email,
            request.InvitedByUserId,
            tokenHash,
            expiresAt);

        var oldInvitations = await _invitationRepository.GetPendingByTenantAndEmailAsync(
            request.TenantId, request.Email, ct);

        foreach (var old in oldInvitations)
        {
            old.Revoke();
            old.ReplacedByInvitationId = newInvitation.Id;
        }

        await _invitationRepository.AddAsync(newInvitation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _mediator.Publish(
            new InvitationCreatedEvent(newInvitation.Id, rawToken, tenant.Slug, request.Email), ct);

        return Result<InvitationResponse>.Success(newInvitation.ToResponse(), _operation);
    }
}
