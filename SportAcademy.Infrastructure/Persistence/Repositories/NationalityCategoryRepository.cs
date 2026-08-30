using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.DTOs.NationalityCategoryDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class NationalityCategoryRepository 
        : BaseRepository<NationalityCategory, int>, INationalityCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public NationalityCategoryRepository(
            ApplicationDbContext context,
            IMapper mapper) 
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<NationalityCategoryDto>> GetAllTranslatedAsync(string lang, CancellationToken cancellationToken = default)
            => await _context.NationalityCategories
                .AsNoTracking()
                .Select(nc => new NationalityCategoryDto
                {
                    Id = nc.Id,
                    Code = nc.Code,
                    Name = nc.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? nc.Name,
                })
                .ToListAsync(cancellationToken);

        public async Task<bool> IsCodeExistAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
            => await _context.NationalityCategories
                .AnyAsync(nc => nc.Code == code && (excludeId == null || nc.Id != excludeId.Value), cancellationToken);

        public async Task<bool> IsNameExistAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
            => await _context.NationalityCategories
                .AnyAsync(nc => nc.Name == name && (excludeId == null || nc.Id != excludeId.Value), cancellationToken);

        public async Task<NationalityCategory?> GetByIdWithTranslationsAsync(int id, CancellationToken cancellationToken = default)
            => await _context.NationalityCategories
                .Include(nc => nc.Translations)
                .FirstOrDefaultAsync(nc => nc.Id == id, cancellationToken);
    }
}
