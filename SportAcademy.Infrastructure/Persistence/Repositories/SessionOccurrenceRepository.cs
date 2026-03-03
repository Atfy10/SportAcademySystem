using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SessionOccurrenceRepository : BaseRepository<SessionOccurrence, int>, ISessionOccurrenceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SessionOccurrenceRepository(ApplicationDbContext context, IMapper mapper ) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedData<SessionOccurrenceCardDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default) => await _context.SessionOccurrences
                .AsNoTracking()
                .ProjectTo<SessionOccurrenceCardDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

    }
}
