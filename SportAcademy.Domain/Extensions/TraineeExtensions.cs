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
        // Delegates to Trainee.GetAge() instead of its own copy of the math - this had its own
        // independent (and buggy: UtcNow + DayOfYear comparison, wrong around a birthday for any
        // tenant not in UTC+0, and wrong across a leap-year boundary) implementation until now.
        public static int CalculateAge(this Trainee trainee) =>
            trainee.GetAge();
        public static bool IsAdult(this Trainee trainee) =>
            trainee.CalculateAge() >= 15;
    }
}
