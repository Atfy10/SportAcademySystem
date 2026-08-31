using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using SportAcademy.Infrastructure.Persistence.Projections;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class FamilyRepository : BaseRepository<Family, int>, IFamilyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentLanguageProvider _languageProvider;

        public FamilyRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ICurrentLanguageProvider languageProvider)
            : base(context, mapper, languageProvider)
        {
            _context = context;
            _mapper = mapper;
            _languageProvider = languageProvider;
        }

        public async Task<IReadOnlyList<TFamilyDto>> SearchFamiliesWithCode<TFamilyDto>(int code, CancellationToken cancellationToken = default) where TFamilyDto : class
            => await _context.Families
                .Where(f => f.FamilyCode == code)
                .ProjectTo<TFamilyDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        public async Task<TFamilyDto?> GetByIdProjectedAsync<TFamilyDto>(int id, CancellationToken cancellationToken = default) where TFamilyDto : class
            => await _context.Families
                .Where(f => f.Id == id)
                .ProjectTo<TFamilyDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<PagedData<FamilyDto>> GetAllPaginatedTranslatedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await _context.Families
                .AsNoTracking()
                .Select(FamilyProjections.ToDto(_languageProvider.Language))
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<FamilyDto?> GetByIdTranslatedAsync(int id, CancellationToken cancellationToken = default)
            => await _context.Families
                .Where(f => f.Id == id)
                .AsNoTracking()
                .Select(FamilyProjections.ToDto(_languageProvider.Language))
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyList<FamilyDto>> SearchFamiliesWithCodeTranslatedAsync(int code, CancellationToken cancellationToken = default)
            => await _context.Families
                .Where(f => f.FamilyCode == code)
                .AsNoTracking()
                .Select(FamilyProjections.ToDto(_languageProvider.Language))
                .ToListAsync(cancellationToken);

        public async Task<(string? Name, string? GuardianName)?> GetTranslatedNamesAsync(int id, string lang, CancellationToken cancellationToken = default)
        {
            var translation = await _context.FamilyTranslations
                .Where(t => t.FamilyId == id && t.LangCode == lang)
                .Select(t => new { t.Name, t.GuardianName })
                .FirstOrDefaultAsync(cancellationToken);

            return translation is null ? null : (translation.Name, translation.GuardianName);
        }

        public async Task<Family?> GetByIdWithTranslationsAsync(int id, CancellationToken cancellationToken = default)
            => await _context.Families
                .Include(f => f.Translations)
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        public int SelectNextId()
        {
            var nextId = _context.Database
                .SqlQueryRaw<int>("SELECT NEXT VALUE FOR FamilyCodeSequence")
                .AsEnumerable()
                .First();

            return nextId;
        }
    }
}
