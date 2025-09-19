using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Extensions
{
    public static class TraineeExtensions
    {
        public static int CalculateAge(this Trainee trainee) =>
            DateTime.Now.Year - trainee.BirthDate.Year - (DateTime.Now.DayOfYear < trainee.BirthDate.DayOfYear ? 1 : 0);
        public static bool IsAdult(this Trainee trainee) =>
            trainee.CalculateAge() >= 15;
    }
}
