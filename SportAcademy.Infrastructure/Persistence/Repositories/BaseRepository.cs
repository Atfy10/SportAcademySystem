using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
        where TEntity : class
        where TKey : notnull
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IMapper _mapper;

        public BaseRepository(ApplicationDbContext context, IMapper mapper = default!)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> IsExistAsync(TKey id, CancellationToken cancellationToken = default)
            => await GetByIdAsync(id, cancellationToken) != null;

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken)
                ?? throw new IdNotFoundException(typeof(TEntity).Name, id.ToString()!);

            _context.Set<TEntity>().Remove(entity);
            await SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Set<TEntity>().Remove(entity);
            await SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);

        public virtual async Task<PagedData<TEntityDto>> GetAllPaginatedAsync<TEntityDto>(PageRequest page, CancellationToken cancellationToken = default)
                where TEntityDto : class
            => await _context.Set<TEntity>()
                .AsNoTracking()
                .ProjectTo<TEntityDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
            => await _context.Set<TEntity>().FindAsync(id, cancellationToken);

        public virtual async Task<TEntity?> GetByIdsAsync(CancellationToken cancellationToken = default, params TKey[] id)
            => await _context.Set<TEntity>().FindAsync(id, cancellationToken);

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Set<TEntity>().Update(entity);
            await SaveChangesAsync(cancellationToken);
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<TEntity> AddAsyncWithoutSave(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task UpdateAsyncWithoutSave(TEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Set<TEntity>().Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsyncWithoutSave(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken)
                ?? throw new IdNotFoundException(typeof(TEntity).Name, id.ToString()!);
            _context.Set<TEntity>().Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsyncWithoutSave(TEntity entity, CancellationToken cancellationToken = default)
        {
            _context.Set<TEntity>().Remove(entity);
            await Task.CompletedTask;
        }
    }
}
