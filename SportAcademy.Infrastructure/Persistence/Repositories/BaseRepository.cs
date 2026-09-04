using AutoMapper;
using AutoMapper.QueryableExtensions;
using SportAcademy.Domain.Contract;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using System.Linq.Expressions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
        where TEntity : class
        where TKey : notnull
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IMapper _mapper;
        /// <summary>
        /// Optional so every existing derived repository keeps compiling unchanged; only the
        /// repositories for translatable entities (Sport, Branch, PaymentType, TraineeGroup,
        /// NationalityCategory) inject and use it, in their own hand-written translated
        /// projections - see SportRepository / SportProjections for the pattern. Not routed
        /// through AutoMapper.ProjectTo: Profiles are configured once at startup, so there is no
        /// way to splice a per-request language into a cached ProjectTo expression tree there.
        /// </summary>
        protected readonly ICurrentLanguageProvider? LanguageProvider;

        public BaseRepository(ApplicationDbContext context, IMapper mapper = default!, ICurrentLanguageProvider? languageProvider = null)
        {
            _context = context;
            _mapper = mapper;
            LanguageProvider = languageProvider;
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
            => await ApplyBranchFilter(_context.Set<TEntity>()).AsNoTracking().ToListAsync(cancellationToken);

        public virtual async Task<PagedData<TEntityDto>> GetAllPaginatedAsync<TEntityDto>(PageRequest page, CancellationToken cancellationToken = default)
                where TEntityDto : class
            => await ApplyPrimaryKeyOrder(ApplyBranchFilter(_context.Set<TEntity>()).AsNoTracking())
                .ProjectTo<TEntityDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        // Entity types like Trainee/Employee/SubscriptionDetails/SportPrice/Payment/Invoice are
        // excluded from ApplicationDbContext's automatic global branch filter (see
        // branchAutoFilterExclusions in OnModelCreating) precisely so that referencing them
        // through an unrelated, already-scoped root doesn't wrongly hide that root. But when one
        // of them itself IS the query's subject - "list all Trainees", "count active Employees",
        // an aggregate stat over SubscriptionDetails - branch restriction still needs to apply,
        // just explicitly here instead of automatically everywhere. Generic (not bound to
        // TEntity) so any repository can apply it to any IBranchScoped entity it queries
        // directly, not only its own TEntity - see TraineeRepository.GetActiveTraineesCount for
        // an example (a Trainee-repository method whose query root is SubscriptionDetails).
        protected IQueryable<T> ApplyBranchFilter<T>(IQueryable<T> query) where T : class
        {
            if (!typeof(IBranchScoped).IsAssignableFrom(typeof(T)))
                return query;

            var parameter = Expression.Parameter(typeof(T), "e");
            var branchIdAccessor = Expression.Property(parameter, nameof(IBranchScoped.BranchId));

            var dbContext = Expression.Constant(_context);
            var isBranchRestricted = Expression.Property(dbContext, nameof(ApplicationDbContext.IsBranchRestricted));
            var allowedBranchIds = Expression.Property(dbContext, nameof(ApplicationDbContext.CurrentAllowedBranchIds));
            var containsCall = Expression.Call(
                typeof(Enumerable), nameof(Enumerable.Contains), [typeof(int)],
                allowedBranchIds, branchIdAccessor);

            var body = Expression.OrElse(Expression.Not(isBranchRestricted), containsCall);
            var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);

            return query.Where(predicate);
        }

        // TEntity carries no compile-time "has an Id" constraint, and not every entity's primary
        // key is even named "Id" (e.g. Coach's is EmployeeId, NotificationRecipient's is
        // composite) - so the tiebreaker Skip/Take pagination needs (see
        // PaginationExtensions.ToPagedDataAsync) is built from EF's own primary-key metadata,
        // the same way the branch/tenant query filters are built in
        // ApplicationDbContext.OnModelCreating.
        private IOrderedQueryable<TEntity> ApplyPrimaryKeyOrder(IQueryable<TEntity> query)
        {
            var keyProperties = _context.Model.FindEntityType(typeof(TEntity))!.FindPrimaryKey()!.Properties;
            var parameter = Expression.Parameter(typeof(TEntity), "e");

            IOrderedQueryable<TEntity>? ordered = null;
            foreach (var keyProperty in keyProperties)
            {
                var propertyAccess = Expression.Convert(Expression.Property(parameter, keyProperty.Name), typeof(object));
                var lambda = Expression.Lambda<Func<TEntity, object>>(propertyAccess, parameter);

                ordered = ordered is null ? query.OrderBy(lambda) : ordered.ThenBy(lambda);
            }

            return ordered!;
        }

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
