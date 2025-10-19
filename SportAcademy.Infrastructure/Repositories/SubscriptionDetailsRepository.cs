using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Repositories
{
    public class SubscriptionDetailsRepository : BaseRepository<SubscriptionDetails, int>, ISubscriptionDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        public SubscriptionDetailsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
