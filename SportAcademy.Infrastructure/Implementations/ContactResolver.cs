using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Implementations;

public class ContactResolver : IContactResolver
{
    private readonly ApplicationDbContext _context;

    public ContactResolver(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ResolveEmailAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await FindUserAsync(userId, ct);
        return user is null ? null : ResolveEmail(user);
    }

    public async Task<string?> ResolvePhoneAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await FindUserAsync(userId, ct);
        return user is null ? null : ResolvePhone(user);
    }

    public async Task<Dictionary<Guid, (string? Email, string? Phone)>> ResolveContactsAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken ct = default)
    {
        if (userIds.Count == 0) return new();

        var users = await _context.Users
            .Include(u => u.Employee)
            .Include(u => u.Trainee)
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(ct);

        return users.ToDictionary(u => u.Id, u => (ResolveEmail(u), ResolvePhone(u)));
    }

    private static string? ResolveEmail(Domain.Entities.AppUser user)
    {
        var employeeEmail = user.Employee?.Email?.ToString();
        if (!string.IsNullOrWhiteSpace(employeeEmail)) return employeeEmail;

        var traineeEmail = user.Trainee?.Email?.ToString();
        if (!string.IsNullOrWhiteSpace(traineeEmail)) return traineeEmail;

        return string.IsNullOrWhiteSpace(user.Email) ? null : user.Email;
    }

    private static string? ResolvePhone(Domain.Entities.AppUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.Employee?.PhoneNumber)) return user.Employee.PhoneNumber;
        if (!string.IsNullOrWhiteSpace(user.Trainee?.PhoneNumber)) return user.Trainee.PhoneNumber;
        return string.IsNullOrWhiteSpace(user.PhoneNumber) ? null : user.PhoneNumber;
    }

    private Task<Domain.Entities.AppUser?> FindUserAsync(Guid userId, CancellationToken ct)
        => _context.Users
            .Include(u => u.Employee)
            .Include(u => u.Trainee)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
}
