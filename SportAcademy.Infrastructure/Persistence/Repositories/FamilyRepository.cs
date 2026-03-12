using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class FamilyRepository : BaseRepository<Family, int>, IFamilyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FamilyRepository(
            ApplicationDbContext context,
            IMapper mapper)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<TFamilyDto>> SearchFamiliesWithCode<TFamilyDto>(int code, CancellationToken cancellationToken = default) where TFamilyDto : class
            => await _context.Families
                .Where(f => f.FamilyCode == code)
                .ProjectTo<TFamilyDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
    }
}
