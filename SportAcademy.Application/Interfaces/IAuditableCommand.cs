namespace SportAcademy.Application.Interfaces
{
    // Implemented by every PlatformCommands command (create/archive/status/plan/feature/
    // subscription/owner-ban/owner-reset-link) so PlatformAuditBehavior can write exactly one
    // TenantAuditEvent per command, in the same transaction as the handler, without each
    // controller action hand-rolling its own audit call (that duplicated call site is what F-05/
    // F-06 found: owner ban and reset-link had none, and every write happened outside the
    // handler's own transaction).
    public interface IAuditableCommand
    {
        /// <summary>Stable, dot-namespaced event key written to TenantAuditEvent.EventType, e.g.
        /// "tenant.archived" - matches the values TenantsController.LogAsync used to hand-type.</summary>
        string AuditEventType { get; }

        /// <summary>
        /// The tenant this event is attributed to. Most commands carry TenantId directly and
        /// just return it. Three don't have one available on the command itself:
        /// CreateTenantCommand (the tenant doesn't exist until the handler creates it) and
        /// BanOwnerCommand/SendOwnerPasswordResetLinkCommand (they target an owner, and the
        /// handler is what resolves the owner's tenant) - those three set a mutable property
        /// from inside their own handler and return it here, since the command instance is the
        /// same object reference the behavior still holds once the handler returns.
        /// </summary>
        Guid? AuditTenantId { get; }

        /// <summary>
        /// Snapshot of the entity's state immediately before the handler's change, written to
        /// TenantAuditEvent.BeforeJson. The handler already loads the entity it's about to
        /// mutate, so it builds a small anonymous object of just the fields this command can
        /// change and assigns it to a settable property (e.g. ResolvedBeforeState) before making
        /// any change - PlatformAuditBehavior reads it back through this property once Handle()
        /// returns, the same pattern AuditTenantId uses for the three commands that resolve their
        /// tenant inside the handler. Null when the command creates a brand-new entity (there is
        /// no "before") or changes nothing an entity snapshot would capture (e.g. sending an
        /// email doesn't mutate the owner record).
        /// </summary>
        object? AuditBeforeState { get; }
    }
}
