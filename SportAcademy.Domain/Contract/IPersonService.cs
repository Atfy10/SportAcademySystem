using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Contract
{
    public interface IPersonService
    {
        bool IsSSNValid(string ssn, DateOnly birthDate);

        /// <summary>Age as of <paramref name="asOf"/> (the caller's own tenant-local "today"),
        /// or the server's local date if omitted - see PersonService's remarks on why a caller
        /// with real tenant context should always pass this explicitly.</summary>
        int CalculateAge(DateOnly birthDate, DateOnly? asOf = null);
        string GenerateUserName(string firstName, string lastName);
        string GeneratePassword();
    }
}
