using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.RequestReconciliationBypass;

// Step 1 of the "refuge" mechanism: a tenant stuck in PendingLimitSelection (wizard unusable for
// some reason) must never be permanently stranded, but reactivating it without resolving the
// over-limit condition is a deliberate, rare, high-trust action - not something one click on
// ChangeTenantStatusCommand should do (see that command's own guard). Sends a 6-digit code to
// the ACTING SuperAdmin's own email, not the tenant's - this proves deliberate intent from
// someone who controls that inbox, the same property invitation email verification already
// establishes for a different actor. No IAuditableCommand here: requesting a code changes
// nothing yet, only the confirm step (which actually flips the tenant) is audited.
public record RequestReconciliationBypassCommand(Guid TenantId) : IRequest<Result>;
