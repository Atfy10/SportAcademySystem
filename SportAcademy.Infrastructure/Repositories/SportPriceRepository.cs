using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.DBContext;

namespace SportAcademy.Infrastructure.Repositories
{
	public class SportPriceRepository : BaseRepository<SportPrice, int>, ISportPriceRepository
	{
		private readonly ApplicationDbContext _context;
		public SportPriceRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
		public async Task<bool> IsKeyExistAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken)
			=> await _context.SportPrices.AnyAsync(
				sp => sp.BranchId == branchId 
					&& sp.SportId == sportId 
					&& sp.SubsTypeId == subsTypeId, cancellationToken);

		public async Task<SportPrice?> GetByKeyAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken)
			=> await _context.SportPrices.FirstOrDefaultAsync(
				sp => sp.BranchId == branchId 
					&& sp.SportId == sportId 
					&& sp.SubsTypeId == subsTypeId, cancellationToken);

		public async Task<List<SportPrice>> GetAllWithIncludesAsync(CancellationToken cancellationToken)
			=> await _context.SportPrices
					.Include(sp => sp.Branch)
					.Include(sp => sp.Sport)
					.Include(sp => sp.SubscriptionType)
					.ToListAsync(cancellationToken);

		public async Task<SportPrice?> GetByKeyWithIncludesAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken)
			=> await _context.SportPrices
					.Include(sp => sp.Branch)
					.Include(sp => sp.Sport)
					.Include(sp => sp.SubscriptionType)
					.FirstOrDefaultAsync(
						sp => sp.BranchId == branchId 
							&& sp.SportId == sportId 
							&& sp.SubsTypeId == subsTypeId, cancellationToken);
	}
}
