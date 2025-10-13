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
		public Task<bool> CheckIfKeyExists(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken)
		=> _context.SportPrices
			.AnyAsync(sp => sp.BranchId == branchId && sp.SportId == sportId && sp.SubsTypeId == subsTypeId, cancellationToken);

		public Task<SportPrice> GetByKeyAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken)
			=> _context.SportPrices
			.FirstOrDefaultAsync(sp => sp.BranchId == branchId && sp.SportId == sportId && sp.SubsTypeId == subsTypeId, cancellationToken);
	}
}
