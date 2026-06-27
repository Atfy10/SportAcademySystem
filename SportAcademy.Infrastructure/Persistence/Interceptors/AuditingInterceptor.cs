using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Persistence.Interceptors
{
    public class AuditingInterceptor : SaveChangesInterceptor
    {
        private readonly string _defaultUser;

        public AuditingInterceptor(IUserContextService contextService)
        {
            _defaultUser = contextService.UserId?.ToString() ?? "System";
        }

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
                        auditableEntity.CreatedBy = _defaultUser;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditableEntity.UpdatedAt = currentTime;
                        auditableEntity.UpdatedBy = _defaultUser;
                    }
                }
            }
        }

    }
}
