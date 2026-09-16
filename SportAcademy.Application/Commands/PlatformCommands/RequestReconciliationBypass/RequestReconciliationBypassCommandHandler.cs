using MediatR;
using SportAcademy.Application.Common.Email;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.PlatformCommands.RequestReconciliationBypass;

public class RequestReconciliationBypassCommandHandler : IRequestHandler<RequestReconciliationBypassCommand, Result>
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContext;
    private readonly IInvitationTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Add.ToString();

    public RequestReconciliationBypassCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IUserContextService userContext,
        IInvitationTokenService tokenService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _userContext = userContext;
        _tokenService = tokenService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RequestReconciliationBypassCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Status != TenantStatus.PendingLimitSelection)
            return Result.Failure(_operation, "This tenant has no limit selection pending.", 400);

        var reconciliation = await _tenantRepository.GetOpenReconciliationAsync(request.TenantId, ct);
        if (reconciliation is null)
            return Result.Failure(_operation, "This tenant has no limit selection pending.", 400);

        var superAdminId = _userContext.UserId
            ?? throw new InvalidOperationException("RequestReconciliationBypassCommand ran without an authenticated caller.");
        var superAdmin = await _userRepository.GetByIdAsync(superAdminId, ct)
            ?? throw new IdNotFoundException(nameof(Domain.Entities.AppUser), superAdminId);

        if (string.IsNullOrWhiteSpace(superAdmin.Email))
            return Result.Failure(_operation, "Your account has no email address on file.", 400);

        var code = _tokenService.GenerateNumericCode();
        var codeHash = _tokenService.HashToken(code);
        reconciliation.SetBypassCode(codeHash, DateTime.UtcNow.Add(CodeLifetime));
        await _unitOfWork.SaveChangesAsync(ct);

        var body = EmailTemplate.Render(
            preheader: $"Your force-reactivate code is {code}",
            heading: "Force-reactivate confirmation",
            bodyHtml: $"""
                <p>You requested to reactivate <strong>{tenant.DisplayName}</strong> without it
                completing its plan-limit selection. This tenant will remain over its limit(s)
                until an Owner or Admin resolves it, or you lower the plan/override again.</p>
                <p>Enter this code to confirm:</p>
                <p style="margin:20px 0;font-size:32px;font-weight:700;letter-spacing:6px;color:#1F242E;">{code}</p>
                """,
            footerNote: "This code expires in 10 minutes. If you didn't request this, you can safely ignore this email - nothing changes unless the code is entered.");

        // The SuperAdmin is actively waiting on this code with no other way to get it -
        // deliberately not caught/swallowed, same reasoning as
        // SendInvitationVerificationCodeCommandHandler.
        await _emailService.SendAsync(superAdmin.Email, "Confirm: force-reactivate a tenant", body, ct);

        return Result.Success(_operation, $"A confirmation code was sent to {superAdmin.Email}.");
    }
}
