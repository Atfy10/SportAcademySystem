using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.TraineeDtos
{
    public record TraineeDto
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string SSN { get; init; } = string.Empty;
        public string? ParentNumber { get; init; }
        public string? GuardianName { get; init; }
        public DateOnly BirthDate { get; init; }
        public string? AppUserId { get; init; }
        public int BranchId { get; init; }
        public HashSet<SportDto> Sports { get; init; } = [];
        public List<EnrollmentDto> Enrollments { get; init; } = [];
    }
}
