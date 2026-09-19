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
        if (user is null) return null;

        var employeeEmail = user.Employee?.Email?.ToString();
        if (!string.IsNullOrWhiteSpace(employeeEmail)) return employeeEmail;

        var traineeEmail = user.Trainee?.Email?.ToString();
        if (!string.IsNullOrWhiteSpace(traineeEmail)) return traineeEmail;

        return string.IsNullOrWhiteSpace(user.Email) ? null : user.Email;
    }

    public async Task<string?> ResolvePhoneAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await FindUserAsync(userId, ct);
        if (user is null) return null;

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
