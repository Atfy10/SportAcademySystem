using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Employee
    {
        public int Id { get; set; }
        public decimal Salary { get; set; }
        public Gender Gender { get; set; }
        public DateTime HireDate { get; set; }
        public required string Address { get; set; }
        public string? SecondPhoneNumber { get; set; }
        public string Position { get; set; } = "Employee";
    }
}
