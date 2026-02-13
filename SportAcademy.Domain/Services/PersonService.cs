using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Services
{
    public class PersonService : IPersonService
    {
        public int CalculateAge(DateOnly birthDate)
        {
            throw new NotImplementedException();
        }

        public static string GenerateUserName(string firstName, string lastName)
        {
            Random random = new Random();
            var userName = $"{firstName.ToLower()}{lastName.ToLower()[..2]}_{Random.Shared.Next(0, 50):D2}";
            return userName;
        }

        public static string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var password = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
            return password;
        }

        public bool IsSSNValid(string ssn, DateOnly birthDate)
        {
            return PersonValidationHelper.IsValidSSN(ssn, birthDate);
        }
    }
}
