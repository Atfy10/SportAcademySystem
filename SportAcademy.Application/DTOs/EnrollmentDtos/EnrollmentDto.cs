using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.EnrollmentDtos
{
    public record EnrollmentDto(
        int Id,
        DateTime EnrollmentDate,
        DateTime ExpiryDate,
        int TraineeId,
        int SessionId
    );
}
