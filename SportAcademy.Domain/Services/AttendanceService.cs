using SportAcademy.Domain.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Services
{
    public class AttendanceService : IAttendanceService
    {
        public AttendanceService()
        {
             
        }
        public bool CanMarkAttendance(int enrollmentId, DateOnly sessionDate)
        {
            return true;
        }

        public bool IsSessionDate(DateOnly attendanceDate, DateOnly sessionDate)
            => attendanceDate == sessionDate;
    }
}
