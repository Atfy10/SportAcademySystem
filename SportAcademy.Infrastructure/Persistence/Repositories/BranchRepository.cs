using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

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
    }
}
