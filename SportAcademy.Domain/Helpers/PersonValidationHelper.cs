using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Helpers
{
    public class PersonValidationHelper
    {
        private PersonValidationHelper() { }

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
        public static string GenerateUserName(string firstName, string lastName)
        {
            var userName = $"{firstName.ToLower().Trim()}{lastName.ToLower().Trim()[..2]}_{Random.Shared.Next(0, 50):D2}";
            return userName;
        }

        public static string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var password = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
            return password;
        }

    }
}
