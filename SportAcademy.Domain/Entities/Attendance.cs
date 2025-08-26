using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Attendance
    {
        public int Id { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
        public TimeOnly CheckInTime { get; set; }
    }
}
