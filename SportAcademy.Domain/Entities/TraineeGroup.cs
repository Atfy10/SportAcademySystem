using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    public class TraineeGroup
    {
        public int Id { get; set; }
        public SkillLevel SkillLevel { get; set; }
        public int MaximumCapacity { get; set; } = 15;
        public int DurationInMinutes { get; set; } = 55;
        public Gender Gender { get; set; }
        public int BranchId { get; set; }
        public int CoachId { get; set; }

        // Navigation Property
        public virtual Branch Branch { get; set; } = null!;
        public virtual Coach Coach { get; set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; set; } = [];
        public virtual ICollection<GroupSchedule> GroupSchedules { get; set; } = [];
    }
}
