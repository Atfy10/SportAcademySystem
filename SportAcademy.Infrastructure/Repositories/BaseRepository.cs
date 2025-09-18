using SportAcademy.Application.Interfaces;
using SportAcademy.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Repositories
{
    public class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
        where TEntity : class
        where TKey : notnull
    {
        private readonly ApplicationDbContext _context;

        public BaseRepository(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
            await SaveChanges();
        }

        public Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity?> GetByIdsAsync(CancellationToken cancellationToken = default, params TKey[] id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
