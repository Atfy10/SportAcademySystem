using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    // IBranchScoped, but excluded from ApplicationDbContext's AUTOMATIC global filter (see
    // branchAutoFilterExclusions in OnModelCreating) - BranchId here is the employee's
    // *employment* branch (HR/payroll), not a data-visibility boundary a coach's teaching
    // assignment should be bound by. A Coach (Employee.Coach) can train groups at branches other
    // than the one that employs them; if Employee were auto-filtered, any query joining through
    // Coach -> Employee (every trainee-group/session/etc. card that shows a coach's name) would
    // silently drop the whole row whenever that coach's employment branch wasn't in the caller's
    // allowed set - even though the group's own branch was. The Employees/Staff list itself
    // still needs branch filtering when Employee is queried as the *subject* of the query, not
    // navigated through - EmployeeRepository applies it explicitly for that case (see
    // ApplyBranchFilter usage there and in BaseRepository.GetAllPaginatedAsync).
    public class Employee : Person, IBranchScoped
    {
        public int Id { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
        public Position Position { get; set; }
        public bool IsWork { get; set; } = true;
        public int BranchId { get; set; }
        public Guid? AppUserId { get; set; }

        // Navigation Property
        public virtual AppUser? AppUser { get; set; }
        public virtual Branch Branch { get; set; } = null!;
        public virtual Coach Coach { get; set; } = null!;
    }
}
