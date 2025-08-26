using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class SportTrainee
    {
        public int SportId { get; set; }
        public int TraineeId { get; set; }
        public required string SkillLevel { get; set; }

        // Navigation Property
        public virtual Sport Sport { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;

    }
}
