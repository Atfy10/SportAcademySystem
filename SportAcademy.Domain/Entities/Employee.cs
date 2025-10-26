using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Employee : Person
    {
        public int Id { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
        public Position Position { get; set; }
        public int BranchId { get; set; }
        public required string AppUserId { get; set; }

        // Navigation Property
        public virtual AppUser AppUser { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual Coach Coach { get; set; } = null!;
    }
}
