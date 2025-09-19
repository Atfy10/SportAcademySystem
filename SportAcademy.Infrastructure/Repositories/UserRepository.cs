using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    public class UserRepository : BaseRepository<AppUser, string>, IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context,
            UserManager<AppUser> userManager) : base(context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => await _userManager.FindByEmailAsync(email);

        public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
            => await _userManager.FindByNameAsync(username);

        public async Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default)
            => await _context.AppUsers.AnyAsync(u => u.Email == email, cancellationToken);

        public async Task<bool> IsUsernameExistAsync(string username, CancellationToken cancellationToken = default)
            => await _context.AppUsers.AnyAsync(u => u.UserName == username, cancellationToken);
    }
}
