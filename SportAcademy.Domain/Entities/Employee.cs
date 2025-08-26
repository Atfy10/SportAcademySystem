using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Employee
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string SSN { get; set; }
        public decimal Salary { get; set; }
        public Gender Gender { get; set; }
        public DateTime HireDate { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber { get; set; }
        public string? SecondPhoneNumber { get; set; }
        public string Position { get; set; } = "Employee";
        public int BranchId { get; set; }
        public required string AppUserId { get; set; }

        // Navigation Property
        public virtual AppUser AppUser { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual Coach Coach { get; set; } = null!;
    }
}
