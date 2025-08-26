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
        // nav For Sport 
        public int SportId { get; set; }

        [ForeignKey(nameof(SportId))]
        public virtual Sport Sport { get; set; }

    }
}
