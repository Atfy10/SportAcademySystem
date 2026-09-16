using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Implementations;

public class LimitSelectionApplier : ILimitSelectionApplier
{
    private readonly ApplicationDbContext _context;

    public LimitSelectionApplier(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LimitSelectionApplyResult> ApplyAsync(
        Guid tenantId, IReadOnlyCollection<int> keepBranchIds, IReadOnlyCollection<int> keepSportIds,
        IReadOnlyCollection<Guid> keepUserIds, CancellationToken ct = default)
    {
        var branches = await _context.Set<Branch>().Where(b => b.TenantId == tenantId).ToListAsync(ct);
        var deactivatedBranchIds = branches.Where(b => !keepBranchIds.Contains(b.Id)).Select(b => b.Id).ToHashSet();
        var deactivatedBranches = 0;
        foreach (var branch in branches)
        {
            var shouldBeActive = keepBranchIds.Contains(branch.Id);
            if (branch.IsActive && !shouldBeActive)
                deactivatedBranches++;
            branch.IsActive = shouldBeActive;
        }

        var sports = await _context.Set<Sport>().Where(s => s.TenantId == tenantId).ToListAsync(ct);
        var deactivatedSports = 0;
        foreach (var sport in sports)
        {
            var shouldBeActive = keepSportIds.Contains(sport.Id);
            if (sport.IsActive && !shouldBeActive)
                deactivatedSports++;
            sport.IsActive = shouldBeActive;
        }

        var users = await _context.Set<AppUser>().Where(u => u.TenantId == tenantId).ToListAsync(ct);
        var bannedUsers = 0;
        foreach (var user in users)
        {
            var shouldStayActive = keepUserIds.Contains(user.Id);
            if (!user.IsBanned && !shouldStayActive)
                bannedUsers++;
            user.IsBanned = !shouldStayActive;
        }

        // D5 cascade, enforced independently of the client's explicit selection: a
        // branch-restricted (Employee-role - see Program.cs's identical role check) user whose
        // UserBranchAccess is now entirely within deactivated branches has nothing left to see,
        // regardless of whether the submitted UserIds already excluded them.
        var cascadeBannedUsers = 0;
        if (deactivatedBranchIds.Count > 0)
        {
            var employeeUserIds = await _context.Set<AppUserRole>()
                .Where(ur => ur.Role.Name == "Employee")
                .Select(ur => ur.UserId)
                .ToListAsync(ct);

            var accessByUser = await _context.Set<UserBranchAccess>()
                .Where(a => a.TenantId == tenantId && employeeUserIds.Contains(a.UserId))
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, BranchIds = g.Select(a => a.BranchId).ToList() })
                .ToListAsync(ct);

            foreach (var entry in accessByUser)
            {
                if (entry.BranchIds.Count == 0 || !entry.BranchIds.All(deactivatedBranchIds.Contains))
                    continue;

                var user = users.FirstOrDefault(u => u.Id == entry.UserId);
                if (user is not null && !user.IsBanned)
                {
                    user.IsBanned = true;
                    cascadeBannedUsers++;
                }
            }

            // Same cascade for coaches: CoachBranchAccess, not UserBranchAccess/Employee.BranchId
            // (a coach's teaching branches are deliberately separate from their employment
            // branch - see CoachBranchAccess's own comment). A coach with no AppUser (rare, but
            // the entity allows it) has no login to ban.
            var coaches = await _context.Set<Coach>()
                .Where(c => c.TenantId == tenantId)
                .Include(c => c.Employee)
                .ToListAsync(ct);

            var accessByCoach = await _context.Set<CoachBranchAccess>()
                .Where(a => a.TenantId == tenantId)
                .GroupBy(a => a.CoachId)
                .Select(g => new { CoachId = g.Key, BranchIds = g.Select(a => a.BranchId).ToList() })
                .ToListAsync(ct);

            foreach (var entry in accessByCoach)
            {
                if (entry.BranchIds.Count == 0 || !entry.BranchIds.All(deactivatedBranchIds.Contains))
                    continue;

                // Coach's own primary key is EmployeeId (1:1 with Employee - see
                // CoachConfiguration), which is what CoachBranchAccess.CoachId references.
                var linkedUserId = coaches.FirstOrDefault(c => c.EmployeeId == entry.CoachId)?.Employee?.AppUserId;
                if (linkedUserId is not { } uid)
                    continue;

                var user = users.FirstOrDefault(u => u.Id == uid);
                if (user is not null && !user.IsBanned)
                {
                    user.IsBanned = true;
                    cascadeBannedUsers++;
                }
            }
        }

        // Every group at a deactivated branch pauses - there is no "which groups to keep" step
        // in the wizard (see PLAN_LIMITS_DESIGN.md D5), this is purely a consequence of the
        // branch choice.
        var deactivatedGroups = 0;
        if (deactivatedBranchIds.Count > 0)
        {
            var groups = await _context.Set<TraineeGroup>()
                .Where(g => g.TenantId == tenantId && deactivatedBranchIds.Contains(g.BranchId) && g.IsActive)
                .ToListAsync(ct);

            foreach (var group in groups)
                group.IsActive = false;
            deactivatedGroups = groups.Count;
        }

        return new LimitSelectionApplyResult(
            deactivatedBranches, deactivatedSports, bannedUsers, cascadeBannedUsers, deactivatedGroups);
    }
}
