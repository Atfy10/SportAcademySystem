using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SportAcademy.Domain.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Interceptors
{
    public class AuditingInterceptor : SaveChangesInterceptor
    {
        override public InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyAuditInformation(eventData);
            return base.SavingChanges(eventData, result);
        }

        override public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation(eventData);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyAuditInformation(DbContextEventData eventData)
        {
            var changeTracker = eventData.Context!.ChangeTracker;
            if (changeTracker != null)
            {
                var entries = changeTracker.Entries()
                    .Where(e => e.Entity is IAuditableEntity
                            && (e.State == EntityState.Added
                            || e.State == EntityState.Modified));

                var currentTime = DateTime.UtcNow;
                foreach (var entry in entries)
                {
                    var auditableEntity = (IAuditableEntity)entry.Entity;
                    if (entry.State == EntityState.Added)
                    {
                        auditableEntity.CreatedAt = currentTime;
                        // auditableEntity.CreatedBy = GetCurrentUserId(); // Implement this method to get the current user ID
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditableEntity.UpdatedAt = currentTime;
                        // auditableEntity.UpdatedBy = GetCurrentUserId(); // Implement this method to get the current user ID
                    }
                }
            }
        }

    }
}
