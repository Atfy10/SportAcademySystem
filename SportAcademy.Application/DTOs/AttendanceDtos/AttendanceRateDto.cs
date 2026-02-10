using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.AttendanceDtos
{
    public record AttendanceRateDto(
        int TraineeId,
        int TotalSessions,
        int AttendedSessions,
        double AttendanceRate
        );
}
