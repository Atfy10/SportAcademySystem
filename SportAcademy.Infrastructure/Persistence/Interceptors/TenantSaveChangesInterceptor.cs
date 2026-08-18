using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Services;

namespace SportAcademy.Infrastructure.Persistence.Interceptors
{
    public class TenantSaveChangesInterceptor : SaveChangesInterceptor
    {
        private Guid _currentTenantId;
        private readonly ITenantIdProvider _tenantIdProvider;

        public TenantSaveChangesInterceptor(ITenantIdProvider tenantIdProvider)
        {
            _tenantIdProvider = tenantIdProvider;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            _currentTenantId = _tenantIdProvider.TenantId ?? Guid.Empty;
            if (_currentTenantId != Guid.Empty)
                ApplyTenantFilter(eventData);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _currentTenantId = _tenantIdProvider.TenantId ?? Guid.Empty;
            if (_currentTenantId != Guid.Empty)
                ApplyTenantFilter(eventData);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyTenantFilter(DbContextEventData eventData)
        {
            var context = eventData.Context;

            foreach (var entry in context?.ChangeTracker.Entries()!)
            {
                if (entry.Entity is ITenantScoped tenantEntity)
                {
                    if (entry.State is EntityState.Added)
                    {
                        tenantEntity.TenantId = _currentTenantId;
                    }
                    else if (entry.State is EntityState.Modified)
                    {
                        if (tenantEntity.TenantId != _currentTenantId)
                        {
                            throw new InvalidOperationException("Cannot change the TenantId of an existing entity.");
                        }
                    }
                }
            }
        }
    }
}

