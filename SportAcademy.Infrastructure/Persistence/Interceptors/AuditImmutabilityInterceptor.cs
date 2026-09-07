using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Interceptors
{
    // Enforces that a TenantAuditEvent, once inserted, can never be changed or removed through
    // this application - a log a SuperAdmin (or a bug) could quietly edit after the fact isn't
    // a log. There is no legitimate code path that updates or deletes one: PlatformAuditBehavior
    // and PlatformDenialAuditResultHandler only ever Add. This interceptor is what turns "no code
    // path does it today" into "no code path ever can," so a future change can't reintroduce a
    // mutable audit trail by accident.
    public class AuditImmutabilityInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            AssertNoMutations(eventData);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            AssertNoMutations(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void AssertNoMutations(DbContextEventData eventData)
        {
            var context = eventData.Context;
            if (context is null) return;

            foreach (var entry in context.ChangeTracker.Entries<TenantAuditEvent>())
            {
                if (entry.State is EntityState.Modified or EntityState.Deleted)
                {
                    throw new InvalidOperationException(
                        "TenantAuditEvent rows are append-only and cannot be modified or deleted.");
                }
            }
        }
    }
}
