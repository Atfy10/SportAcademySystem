using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Contract
{
    public interface IAttendanceService
    {
        bool CanMarkAttendance(int enrollmentId, DateOnly sessionDate);
        bool IsSessionDate(DateOnly attendanceDate, DateOnly sessionDate);
    }
}
