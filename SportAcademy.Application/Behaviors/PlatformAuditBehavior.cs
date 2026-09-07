using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Behaviors
{
    // Writes exactly one TenantAuditEvent per IAuditableCommand, in the SAME database
    // transaction as the handler it audits - the fix for F-06 (audit writes used to happen in
    // their own separate SaveChanges call, after the handler had already committed, so a failed
    // audit insert never rolled back the business change it was supposed to record) and F-05
    // (owner ban / password-reset-link had no audit call site at all; they get one for free now
    // just by implementing the interface).
    //
    // How the atomicity works: this behavior opens the transaction BEFORE calling the handler,
    // so the handler's own (unmodified) call to IUnitOfWork.SaveChangesAsync() - every handler
    // in scope already makes exactly one - participates in that same ambient transaction instead
    // of opening its own. Once the handler returns, the outcome is known, so the audit event is
    // staged on the same DbContext and flushed with one more SaveChangesAsync() before the
    // transaction commits. No handler needed to change to get this guarantee.
    //
    // BeforeJson comes from IAuditableCommand.AuditBeforeState, which each handler populates
    // itself (it already loads the entity it's about to mutate) before making any change - this
    // behavior only serializes whatever the handler left there. AfterJson stays the command
    // payload itself: a uniform, mechanical capture that's honest for all of them, whereas a
    // generic "after" entity snapshot would need the same per-handler plumbing as BeforeJson for
    // comparatively little benefit (the payload already says what was requested).
    public class PlatformAuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : ResultBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantAuditRepository _auditRepository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PlatformAuditBehavior<TRequest, TResponse>> _logger;

        public PlatformAuditBehavior(
            IUnitOfWork unitOfWork,
            ITenantAuditRepository auditRepository,
            IUserContextService userContext,
            IUserRepository userRepository,
            ILogger<PlatformAuditBehavior<TRequest, TResponse>> logger)
        {
            _unitOfWork = unitOfWork;
            _auditRepository = auditRepository;
            _userContext = userContext;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (request is not IAuditableCommand auditable)
                return await next(ct);

            var userId = _userContext.UserId
                ?? throw new InvalidOperationException(
                    "PlatformAuditBehavior ran for an unauthenticated request - every " +
                    "IAuditableCommand is reached through a SuperAdmin-only, [Authorize]'d " +
                    "route, so a missing UserId here means that guarantee was broken upstream.");

            await _unitOfWork.BeginTransactionAsync(ct);

            TResponse response;
            try
            {
                response = await next(ct);
            }
            catch
            {
                // No audit event on an unhandled exception: ExceptionHandlingBehavior (which
                // wraps this behavior) already logs it with full context and a correlation id -
                // this log is for normal business outcomes, not exception telemetry.
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }

            var actorName = await _userRepository.GetDisplayNameAsync(userId, ct);

            var auditEvent = new TenantAuditEvent
            {
                TenantId = auditable.AuditTenantId,
                EventType = auditable.AuditEventType,
                Description = response.Message,
                Outcome = response.IsSuccess ? AuditOutcome.Succeeded : AuditOutcome.Failed,
                BeforeJson = auditable.AuditBeforeState is not null
                    ? JsonSerializer.Serialize(auditable.AuditBeforeState)
                    : null,
                AfterJson = JsonSerializer.Serialize(request),
                PerformedByUserId = userId,
                PerformedBy = actorName,
                IpAddress = _userContext.IpAddress,
                UserAgent = _userContext.UserAgent,
                CorrelationId = Activity.Current?.TraceId.ToString(),
                PerformedAt = DateTime.UtcNow,
            };

            await _auditRepository.AddWithoutSaveAsync(auditEvent, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            _logger.LogInformation(
                "Platform audit: {EventType} by {UserId} against tenant {TenantId} - {Outcome}",
                auditEvent.EventType, userId, auditEvent.TenantId, auditEvent.Outcome);

            return response;
        }
    }
}
