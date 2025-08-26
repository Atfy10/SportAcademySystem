using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Coach
    {
        public required string SkillLevel { get; set; }
        public int EmployeeId { get; set; }
        public int SportId { get; set; }
        public DateOnly BirthDate { get; set; }

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Sport Sport { get; set; } = null!;
    }
}
