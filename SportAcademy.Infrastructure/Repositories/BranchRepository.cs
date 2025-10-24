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
    public class BranchRepository : BaseRepository<Branch, int>, IBranchRepository
    {
        private readonly ApplicationDbContext _context;
        public BranchRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsCoordinatesExistAsync(string coX, string coY, CancellationToken cancellationToken)
            => await _context.Branchs
                .AnyAsync(b => b.CoX == coX && b.CoY == coY, cancellationToken);

        public async Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken)
            => await _context.Branchs
                .AnyAsync(b => b.Email == email, cancellationToken);

        public async Task<bool> IsPhoneNumberExistAsync(string phoneNumber, CancellationToken cancellationToken)
            => await _context.Branchs
                .AnyAsync(b => b.PhoneNumber == phoneNumber, cancellationToken);
	}
}
