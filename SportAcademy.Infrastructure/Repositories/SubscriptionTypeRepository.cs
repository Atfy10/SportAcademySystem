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
	public class SubscriptionTypeRepository : BaseRepository<SubscriptionType, int>, ISubscriptionTypeRepository
	{
		private readonly ApplicationDbContext _context;

		public SubscriptionTypeRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
