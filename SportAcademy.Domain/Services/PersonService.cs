using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Domain.Services
{
    public class PersonService : IPersonService
    {
        public int CalculateAge(DateOnly birthDate)
        {
            // DateTime.Now (local), not UtcNow: birthDate is a calendar concept, and comparing
            // it against a UTC instant's calendar date is wrong by exactly one day - and
            // therefore one year, right around a birthday - for any tenant not in UTC+0 (this
            // app's primary market is Arabic-region, UTC+2/+3, so this was live-broken there).
            // Full-date comparison (not DayOfYear) also sidesteps a leap-year mismatch: Feb 29
            // and any day after it don't line up across a leap/non-leap year pair under
            // DayOfYear alone. Same algorithm as Trainee.GetAge(), the one call path in this
            // codebase that already got this right.
            var today = DateOnly.FromDateTime(DateTime.Now);
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age;
        }

        public string GenerateUserName(string firstName, string lastName)
        {
            var userName = $"{firstName.ToLower().Trim()}{lastName.ToLower().Trim()[..2]}_{Random.Shared.Next(0, 50):D2}";
            return userName;
        }

        public string GeneratePassword()
        {
            // Guarantees at least one char from each category the Identity password policy
            // requires (upper/lower/digit/non-alphanumeric) instead of hoping 12 random picks
            // happen to cover all four - a random-only draw could silently fail policy
            // validation and abort the whole employee/user creation.
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()";
            const string all = upper + lower + digits + special;
            const int length = 12;

            var passwordChars = new char[length];
            passwordChars[0] = upper[Random.Shared.Next(upper.Length)];
            passwordChars[1] = lower[Random.Shared.Next(lower.Length)];
            passwordChars[2] = digits[Random.Shared.Next(digits.Length)];
            passwordChars[3] = special[Random.Shared.Next(special.Length)];

            for (var i = 4; i < length; i++)
                passwordChars[i] = all[Random.Shared.Next(all.Length)];

            // Shuffle so the guaranteed characters aren't always in positions 0-3.
            for (var i = passwordChars.Length - 1; i > 0; i--)
            {
                var j = Random.Shared.Next(i + 1);
                (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
            }

            return new string(passwordChars);
        }

        public bool IsSSNValid(string ssn, DateOnly birthDate)
        {
            return PersonValidationHelper.IsValidSSN(ssn, birthDate);
        }
    }
}
