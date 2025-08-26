using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Trainee
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string SSN { get; set; }
        public bool IsSubscribed { get; set; }
        public string? ParentNumber { get; set; }
        public string? GuardianName { get; set; }
        public DateOnly BirthDate { get; set; }
        public Gender Gender { get; set; }
        public required string AppUserId { get; set; }

        // Navigation Properties
        public virtual AppUser AppUser { get; set; } = null!;
        public virtual ICollection<SportTrainee> Sports { get; set; } = [];
        public virtual ICollection<Enrollment> Enrollments { get; set; } = [];
        public virtual ICollection<SubscriptionDetails> SubscriptionDetails { get; set; } = [];
    }
}
