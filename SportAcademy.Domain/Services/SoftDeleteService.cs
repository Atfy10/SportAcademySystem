using SportAcademy.Domain.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Services
{
    public static class SoftDeleteService
    {
        public static void MarkAsDeleted<TEntity>(this TEntity entity, string user = "System")
            where TEntity : ISoftDeletable
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = user;
        }

        public static void RestoreFromDeleted<TEntity>(this TEntity entity)
            where TEntity : ISoftDeletable
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
        }
    }
}
