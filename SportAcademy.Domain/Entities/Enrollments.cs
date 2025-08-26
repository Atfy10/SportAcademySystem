using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Enrollments
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
