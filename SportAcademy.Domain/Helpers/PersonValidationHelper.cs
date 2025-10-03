using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Helpers
{
    public class PersonValidationHelper
    {
        public static bool IsValidSSN(string ssn, DateOnly birthDate)
        {
            // Example validation: SSN must be exactly 9 digits long and contain only numbers
            if (ssn.Length != 12 || !ssn.All(char.IsDigit))
                return false;

            // 3 04 03 03
            var year = birthDate.Year > 1999 ? 3 : 2;
            var sixNumbersOfBirth = birthDate.ToString("yyMMdd");

            if (!ssn.StartsWith($"{year}{sixNumbersOfBirth}"))
                return false;

            return true;
        }
    }
}
