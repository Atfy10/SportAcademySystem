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
        //Name Prop
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        //SSN Prop
        public required string SSN { get; set; }
        //Phone Number Prop
        public string PhoneNumber { get; set; }
        public decimal Salary { get; set; }
        public Gender Gender { get; set; }
        public DateTime HireDate { get; set; }
        public required string Address { get; set; }
        public string? SecondPhoneNumber { get; set; }
        public string Position { get; set; } = "Employee";
        // nav For Branch 
        public int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; }
    }
}
