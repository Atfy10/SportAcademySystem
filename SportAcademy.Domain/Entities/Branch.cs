using SportAcademy.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    public class Branch
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public required string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public required Coordinate Coordinate { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<TraineeGroup> TraineeGroups { get; set; } = [];
        public virtual ICollection<Employee> Employees { get; set; } = [];
        public virtual ICollection<SportBranch> Sports { get; set; } = [];
        public virtual ICollection<SportPrice> SportPrices { get; set; } = [];
        public virtual ICollection<Payment> Payments { get; set; } = [];
    }
}
