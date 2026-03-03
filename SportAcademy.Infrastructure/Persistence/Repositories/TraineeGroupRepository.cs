using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class TraineeGroupRepository : BaseRepository<TraineeGroup, int>, ITraineeGroupRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TraineeGroupRepository(ApplicationDbContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<int> GetActiveTraineeGroupsCountAsync(CancellationToken cancellationToken = default) =>
            await _context.TraineeGroups
                .AsNoTracking()
                .Where(tg => tg.Enrollments.Any(e => e.IsActive))
                .CountAsync(cancellationToken);

        public async Task<PagedData<TraineeGroupCardDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default)=>
           await _context.TraineeGroups
                .AsNoTracking()
                .ProjectTo<TraineeGroupCardDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);
  

        public async Task<PagedData<ListTraineeGroupDto>> GetAllOfSpecificDayAsync(PageRequest page, DateTime day, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .AsNoTracking()
                .Where(tg => tg.GroupSchedules.Any(gs => gs.Day == day.DayOfWeek))
                .Select(tg => new ListTraineeGroupDto(
                    tg.Id,
                    tg.Coach.Sport.Name,
                    tg.Coach.Employee.FirstName,
                    tg.Branch.Name,
                    tg.DurationInMinutes,
                    tg.Enrollments.Count,
                    tg.GroupSchedules.FirstOrDefault().StartTime 
                ))
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<int> GetAllTraineeGroupsCountAsync(CancellationToken cancellationToken = default) => await _context.TraineeGroups.CountAsync(cancellationToken);

    }
}

