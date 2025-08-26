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
        public string? ParentNumber { get; set; }
        public string? GuardianName { get; set; }
        public Gender Gender { get; set; }
    }
}
