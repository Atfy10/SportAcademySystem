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
	public class SportRepository : BaseRepository<Sport,int>, ISportRepository
	{
		private readonly ApplicationDbContext _context;
		public SportRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<bool> CheckIfSportExists(int sportId, CancellationToken cancellationToken)
			=> await _context.Sports
				.AnyAsync(s => s.Id == sportId, cancellationToken);

	}
}
