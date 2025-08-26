using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class SportBranch
    {
        public int SportId { get; set; }
        public int BranchId { get; set; }

        // Navigation Property
        public virtual Sport Sport { get; set; }
        public virtual Branch Branch { get; set; }
    }
}
