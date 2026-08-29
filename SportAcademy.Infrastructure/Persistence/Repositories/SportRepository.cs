using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using SportAcademy.Infrastructure.Persistence.Projections;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SportRepository : BaseRepository<Sport, int>, ISportRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentLanguageProvider _languageProvider;

        public SportRepository(ApplicationDbContext context, IMapper mapper, ICurrentLanguageProvider languageProvider)
            : base(context, mapper, languageProvider)
        {
            _context = context;
            _mapper = mapper;
            _languageProvider = languageProvider;
        }

        public async Task<IEnumerable<Sport>> GetAvailableSportsForBranch(int branchId, CancellationToken cancellationToken)
            => await _context.Sports
                .Where(s => !s.Branches.Any(sb => sb.SportId == s.Id && sb.BranchId == branchId))
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<SportDto>> GetAvailableSportsForBranchTranslatedAsync(int branchId, CancellationToken cancellationToken)
            => await _context.Sports
                .Where(s => !s.Branches.Any(sb => sb.SportId == s.Id && sb.BranchId == branchId))
                .Select(SportProjections.ToDto(_languageProvider.Language))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<bool> IsExistByNameAsync(string name, CancellationToken cancellationToken = default)
           => await _context.Sports
               .AnyAsync(s => s.Name == name, cancellationToken);

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return await _context.Sports.CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SportDropDownListDto>> SearchNameAsync(string term, CancellationToken cancellationToken = default)
        {
            var pattern = $"%{term}%";

            return await _context.Sports
                .Where(s => EF.Functions.Like(s.Name, pattern))
                .Select(SportProjections.ToDropDownDto(_languageProvider.Language))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedData<SportDto>> SearchAsync(string term, PageRequest page, CancellationToken cancellationToken = default)
            => await _context.Sports
                .Where(s => EF.Functions.Like(s.Name, $"%{term}%"))
                .Select(SportProjections.ToDto(_languageProvider.Language))
                .AsNoTracking()
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<bool> AreIdsExistAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            var idList = ids.ToList();
            if (!idList.Any()) return true;
            var existingCount = await _context.Sports
                .Where(s => idList.Contains(s.Id))
                .CountAsync(cancellationToken);
            return existingCount == idList.Count;
        }

        public async Task<IReadOnlyList<SportDto>> GetAllTranslatedAsync(CancellationToken cancellationToken = default)
            => await _context.Sports
                .AsNoTracking()
                .Select(SportProjections.ToDto(_languageProvider.Language))
                .ToListAsync(cancellationToken);

        public async Task<PagedData<SportDto>> GetAllPaginatedTranslatedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await _context.Sports
                .AsNoTracking()
                .Select(SportProjections.ToDto(_languageProvider.Language))
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<SportDto?> GetTranslatedByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _context.Sports
                .Where(s => s.Id == id)
                .Select(SportProjections.ToDto(_languageProvider.Language))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
    }
}
