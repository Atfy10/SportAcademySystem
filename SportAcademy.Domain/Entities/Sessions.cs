using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Sessions
    {
        public int Id { get; set; }
        public DayOfWeek Day { get; set; }
        public required string SkillLevel { get; set; }
        public int MaximumCapacity { get; set; } = 15;
        public TimeOnly StartTime { get; set; }
        public DateOnly Date { get; set; }
        public int DurationInMinutes { get; set; } = 55;
        public required string Gender { get; set; }
    }
}
