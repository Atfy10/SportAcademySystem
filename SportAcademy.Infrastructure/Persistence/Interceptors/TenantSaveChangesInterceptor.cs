using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Services;

namespace SportAcademy.Infrastructure.Persistence.Interceptors
{
    public class TenantSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ITenantIdProvider _tenantIdProvider;

        public TenantSaveChangesInterceptor(ITenantIdProvider tenantIdProvider)
        {
            _tenantIdProvider = tenantIdProvider;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyTenantFilter(eventData);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyTenantFilter(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // No ambient tenant AND no explicit AllowCrossTenantOperation() scope is never treated
        // as "skip the check" any more - every entity here is either stamped/verified against a
        // real tenant or the save is rejected outright. A blank ambient tenant used to silently
        // fall through (checking `_currentTenantId != Guid.Empty` before doing anything at all),
        // which meant a bug that ever let a request reach SaveChanges without a resolved tenant
        // - the one guarantee the tenant-claim 400 gate in Program.cs is supposed to provide -
        // would write (or overwrite) rows with no real tenant ownership instead of failing loudly.
        private void ApplyTenantFilter(DbContextEventData eventData)
        {
            var context = eventData.Context;
            if (context is null) return;

            var ambientTenantId = _tenantIdProvider.TenantId;
            var crossTenantAllowed = _tenantIdProvider.AllowCrossTenantWrite;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is not ITenantScoped tenantEntity) continue;
                if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;

                if (ambientTenantId is { } tid)
                {
                    if (entry.State is EntityState.Added)
                    {
                        tenantEntity.TenantId = tid;
                    }
                    else if (tenantEntity.TenantId != tid)
                    {
                        throw new InvalidOperationException("Cannot change the TenantId of an existing entity.");
                    }

                    continue;
                }

                if (crossTenantAllowed)
                {
                    // Explicitly acknowledged cross-tenant batch (a background sweep): every row
                    // must already carry its own real tenant, set by whatever query fetched it
                    // (or, for a new row, by the caller) - never left for this interceptor to
                    // invent, since there is no single ambient tenant to invent it from.
                    if (tenantEntity.TenantId == Guid.Empty)
                    {
                        throw new InvalidOperationException(
                            $"Cross-tenant write of {entry.Entity.GetType().Name} is missing a TenantId - " +
                            "AllowCrossTenantOperation() does not stamp one for you.");
                    }

                    continue;
                }

                throw new InvalidOperationException(
                    $"Refusing to save {entry.Entity.GetType().Name}: no tenant is resolved for this " +
                    "operation. Every write to tenant-scoped data must run with an ambient tenant " +
                    "(set by request middleware) or inside an explicit AllowCrossTenantOperation() scope.");
            }
        }
    }
}

