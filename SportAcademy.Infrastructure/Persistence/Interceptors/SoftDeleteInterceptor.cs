using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly IUserContextService _contextService;
        private readonly string _defaultUser;

        public SoftDeleteInterceptor(IUserContextService contextService)
        {
            _contextService = contextService;
            _defaultUser = _contextService.UserId ?? "Admin";
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            var context = eventData.Context;

            foreach (var entry in context?.ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Deleted)!)
            {
                if (entry.Entity is ISoftDeletable entity)
                {
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedBy = _defaultUser;
                }
            }

            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;

            foreach (var entry in context?.ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Deleted)!)
            {
                if (entry.Entity is ISoftDeletable entity)
                {
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedBy = _defaultUser;
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

    }
}
