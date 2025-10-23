using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Extensions;
using SportAcademy.Infrastructure.DBContext;

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

        public override async Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            await _userManager.CreateAsync(user, $"{user.UserName}1234");
        }

        public override async Task UpdateAsync(AppUser entity, CancellationToken cancellationToken = default)
        {
            await _userManager.UpdateAsync(entity);
        }

        public override async Task DeleteAsync(AppUser entity, CancellationToken cancellationToken = default)
        {
            await _userManager.DeleteAsync(entity);
        }

        public override async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            await _userManager.DeleteAsync(await GetByIdAsync(id, cancellationToken)
                ?? throw new IdNotFoundException(EntityTypes.User.DisplayName(), id.ToString()));
        }

        public override async Task<AppUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => await _userManager.FindByIdAsync(id);

        public override async Task<List<AppUser>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _userManager.Users.ToListAsync(cancellationToken);

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
