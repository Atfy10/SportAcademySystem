using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.AttendanceDtos
{
    public record AttendanceDto(
        int Id,
        DateTime AttendanceDate,
        string AttendanceStatus,
        string CheckInTime,
        string CoachNote,
        int EnrollmentId,
        int SessionOccurrenceId
        ); 
}
