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
	public class SportTraineeRepository : BaseRepository<SportTrainee, int>, ISportTraineeRepository
	{
		private readonly ApplicationDbContext _context;

		public SportTraineeRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
		public async Task<bool> CheckIfKeyExists(int sportId, int traineeId, CancellationToken cancellationToken)
		=> await _context.SportTrainees.AnyAsync(st => st.SportId == sportId && st.TraineeId == traineeId, cancellationToken);

		public async Task<SportTrainee?> GetByIdAsync(int sportId, int traineeId, CancellationToken cancellationToken)
			=> await _context.SportTrainees.FirstOrDefaultAsync(st => st.SportId == sportId && st.TraineeId == traineeId, cancellationToken);

		public async Task<List<SportTrainee>> GetAllAsyncWithIncludeAsync(CancellationToken cancellationToken)
			=> await _context.SportTrainees
				.Include(st => st.Sport)
				.Include(st => st.Trainee)
				.ToListAsync(cancellationToken);

		public async Task<SportTrainee?> GetByIdWithIncludesAsync(int sportId, int traineeId, CancellationToken cancellationToken)
			=> await _context.SportTrainees
				.Include(st => st.Sport)
				.Include(st => st.Trainee)
				.FirstOrDefaultAsync(st => st.SportId == sportId && st.TraineeId == traineeId, cancellationToken);

	}
}
