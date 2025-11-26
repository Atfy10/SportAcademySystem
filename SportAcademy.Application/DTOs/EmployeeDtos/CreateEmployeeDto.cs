using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.EmployeeDtos
{
    public record CreateEmployeeDto
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string SSN { get; init; } = null!;
        public decimal Salary { get; init; }
        public Gender Gender { get; init; }
        public DateOnly BirthDate { get; init; }
        public string Address { get; init; } = null!;
        public string PhoneNumber { get; init; } = null!;
        public string? SecondNumber { get; init; }
        public Position Position { get; init; }
        public int BranchId { get; init; }
        public string AppUserId { get; init; } = null!;
    }
}
