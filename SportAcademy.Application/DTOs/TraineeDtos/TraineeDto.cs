using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.TraineeDtos
{
    public class TraineeDto
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string SSN { get; set; }
        public string? ParentNumber { get; set; }
        public string? GuardianName { get; set; }
        public DateOnly BirthDate { get; set; }
        public string? AppUserId { get; set; }
        public int BranchId { get; set; }
        public HashSet<SportName> Sports { get; set; } = [];
        public List<int> Enrollments { get; set; } = [];

    }
}
