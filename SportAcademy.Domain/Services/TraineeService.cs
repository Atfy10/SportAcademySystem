using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Services
{
    public class TraineeService : ITraineeService
    {
        public int CalculateAge(DateOnly birthDate) =>
            DateTime.Now.Year - birthDate.Year - (DateTime.Now.DayOfYear < birthDate.DayOfYear ? 1 : 0);

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

        public bool IsAdult(DateOnly birthDate) =>
            CalculateAge(birthDate) >= 15;

        public bool IsSSNValid(string ssn, DateOnly birthDate)
        {
            return PersonValidationHelper.IsValidSSN(ssn, birthDate);
        }
    }
}
