using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SportAcademy.Domain.Entities
{
    internal class Enrollment
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int TraineeId { get; set; }
        public int SessionId { get; set; }

        // Navigation Properties
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual Session Session { get; set; } = null!;
        public virtual ICollection<Attendance> Attendances { get; set; } = [];
    }
}
