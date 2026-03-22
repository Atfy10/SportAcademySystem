using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class BranchRepository : BaseRepository<Branch, int>, IBranchRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BranchRepository(
            ApplicationDbContext context,
            IMapper mapper
        ) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<BranchDropDownListDto>> GetAllBranchsBase(CancellationToken cancellationToken = default)
            => await _context.Branchs
                .AsNoTracking()
                .ProjectTo<BranchDropDownListDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        public async Task<int> GetBranchesCountAsync(CancellationToken cancellationToken = default)
            => await _context.Branchs.CountAsync(cancellationToken);

        public async Task<bool> IsCoordinatesExistAsync(string coX, string coY, CancellationToken cancellationToken = default)
            => await _context.Branchs
                .AnyAsync(b => b.CoX == coX && b.CoY == coY, cancellationToken);

        public async Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default)
            => await _context.Branchs
                .AnyAsync(b => b.Email == email, cancellationToken);

        public async Task<bool> IsPhoneNumberExistAsync(string phoneNumber, CancellationToken cancellationToken = default)
            => await _context.Branchs
                .AnyAsync(b => b.PhoneNumber == phoneNumber, cancellationToken);

        public async Task<int> GetBranchTotalCapacityAsync(int branchId, CancellationToken ct)
            => await _context.TraineeGroups
                .Where(g => g.BranchId == branchId)
                .SumAsync(g => g.MaximumCapacity, ct);

        public async Task<PagedData<BranchCardDto>> GetAllPaginatedAsync(PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.Branchs
                .OrderBy(b => b.Id)
                .AsNoTracking()
                .ProjectTo<BranchCardDto>(_mapper.ConfigurationProvider);

            return await query.ToPagedDataAsync(page, cancellationToken);
        }

        public async Task<PagedData<BranchCardDto>> SearchAsync(string term, PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.Branchs
                .Where(b => b.Name.Contains(term) || b.City.Contains(term) || b.Country.Contains(term))
                .OrderBy(b => b.Id)
                .AsNoTracking()
                .ProjectTo<BranchCardDto>(_mapper.ConfigurationProvider);

            return await query.ToPagedDataAsync(page, cancellationToken);
        }

        public async Task<BranchStatsDto> GetBranchStatsAsync(int branchId, CancellationToken cancellationToken = default)
        {
            var totalTrainees = await _context.Enrollments
                .Where(e => e.TraineeGroup!.BranchId == branchId && e.IsActive)
                .Select(e => e.TraineeId)
                .Distinct()
                .CountAsync(cancellationToken);

            var totalCoaches = await _context.Coachs
                .Where(c => c.Employee!.BranchId == branchId)
                .CountAsync(cancellationToken);

            var activeGroups = await _context.TraineeGroups
                .Where(g => g.BranchId == branchId)
                .CountAsync(cancellationToken);

            var today = DateTime.Today;
            var activeSessions = await _context.SessionOccurrences
                .Where(s => s.GroupSchedule!.TraineeGroup!.BranchId == branchId
                    && s.StartDateTime.Date == today
                    && s.Status != SessionStatus.Canceled)
                .CountAsync(cancellationToken);

            return new BranchStatsDto
            {
                TotalTrainees = totalTrainees,
                TotalCoaches = totalCoaches,
                ActiveGroups = activeGroups,
                ActiveSessions = activeSessions
            };
        }

        public async Task<bool> ToggleIsActiveAsync(int id, CancellationToken cancellationToken = default)
        {
            var branch = await _context.Branchs.FindAsync(new object[] { id }, cancellationToken);
            if (branch == null)
                return false;

            branch.IsActive = !branch.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return branch.IsActive;
        }
    }
}
