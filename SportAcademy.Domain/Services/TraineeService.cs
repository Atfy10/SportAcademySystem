using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Domain.Services
{
    public class TraineeService : ITraineeService
    {
        public int CalculateAge(DateOnly birthDate, DateOnly? asOf = null)
        {
            // Full-date comparison, not UtcNow + DayOfYear - see PersonService.CalculateAge's
            // identical remarks for why, and for why a caller with real tenant context should
            // always pass asOf explicitly rather than rely on the DateTime.Now fallback below.
            var today = asOf ?? DateOnly.FromDateTime(DateTime.Now);
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age;
        }

        public int CreateTraineeCode(Trainee trainee, int branchId)
        {
            var year = (trainee.BirthDate.Year % 100);
            var month = (trainee.BirthDate.Month);
            var dobCode = $"{year:D2}{month:D2}";

            var firstLetter = char.ToUpper(trainee.FirstName[0]);
            var ascii = ((int)firstLetter).ToString("D2");

            var prefix = $"{branchId}{dobCode}{ascii}";

            var count = 0; // count of trianees with same prefix

            var counter = (count + 1).ToString("D2");

            var codeString = $"{prefix}{counter}";
            return int.Parse(codeString);
        }

        public bool IsAdult(DateOnly birthDate, DateOnly? asOf = null) =>
            CalculateAge(birthDate, asOf) >= 15;

        public bool IsSSNValid(string ssn, DateOnly birthDate)
        {
            return PersonValidationHelper.IsValidSSN(ssn, birthDate);
        }
    }
}
