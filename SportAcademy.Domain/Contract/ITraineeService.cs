using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Contract
{
    public interface ITraineeService
    {
        int CreateTraineeCode(Trainee trainee, int branchId);
        bool IsSSNValid(string ssn, DateOnly birthDate);

        /// <summary>Age as of <paramref name="asOf"/> (the caller's own tenant-local "today"),
        /// or the server's local date if omitted - see IPersonService.CalculateAge's identical
        /// remark on why a caller with real tenant context should always pass this.</summary>
        int CalculateAge(DateOnly birthDate, DateOnly? asOf = null);
        bool IsAdult(DateOnly birthDate, DateOnly? asOf = null);
    }
}
