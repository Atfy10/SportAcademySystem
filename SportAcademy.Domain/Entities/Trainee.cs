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
        //Name Prop
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // SSN Prop 
        public string SSN { get; set; }
        // Status Prop 
        public string Status { get; set; }
        public string? ParentNumber { get; set; }
        public string? GuardianName { get; set; }
        public Gender Gender { get; set; }
    }
}
