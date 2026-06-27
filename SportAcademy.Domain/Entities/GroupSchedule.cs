using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    public class GroupSchedule : ITenantScoped
    {
        public int Id { get; set; }
        public int TraineeGroupId { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeOnly StartTime { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual TraineeGroup TraineeGroup { get; set; } = null!;
        public virtual ICollection<SessionOccurrence> SessionOccurrences { get; set; } = [];
    }
}
