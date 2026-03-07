using AutoMapper;
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
    }
}
