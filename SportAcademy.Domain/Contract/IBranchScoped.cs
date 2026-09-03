namespace SportAcademy.Domain.Contract;

/// Marks an entity that belongs to exactly one Branch, restrictable per-user via
/// IBranchAccessProvider - see ApplicationDbContext.OnModelCreating for how this drives a
/// global query filter, same mechanism as ITenantScoped/TenantId one level up.
public interface IBranchScoped
{
    int BranchId { get; set; }
}
