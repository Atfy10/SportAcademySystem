using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Domain.Services
{
    public class PersonService : IPersonService
    {
        public int CalculateAge(DateOnly birthDate)
            => DateOnly.FromDateTime(DateTime.Now).Year - birthDate.Year - 
               (DateOnly.FromDateTime(DateTime.Now) < birthDate.AddYears(DateOnly.FromDateTime(DateTime.Now).Year - birthDate.Year) ? 1 : 0);

        public string GenerateUserName(string firstName, string lastName)
        {
            var userName = $"{firstName.ToLower().Trim()}{lastName.ToLower().Trim()[..2]}_{Random.Shared.Next(0, 50):D2}";
            return userName;
        }

        public string GeneratePassword()
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
