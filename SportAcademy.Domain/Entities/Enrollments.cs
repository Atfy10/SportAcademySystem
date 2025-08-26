using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SportAcademy.Domain.Entities
{
    internal class Enrollments
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        // nav For Trainee  
        public int TraineeId { get; set; }

        [ForeignKey(nameof(TraineeId))]
        public virtual Trainee Trainee { get; set; }
        // nav For Session  
        public int SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Sessions Session { get; set; }
    }
}
