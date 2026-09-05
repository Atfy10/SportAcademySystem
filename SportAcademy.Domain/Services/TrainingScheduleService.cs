using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Domain.Services
{
    public static class TrainingScheduleService
    {
        // How long a subscription actually lasts depends on two things, not one: how many
        // sessions it buys (the subscription type) and which days of the week they can be used
        // on (the group's schedule). Two trainees on the same plan finish weeks apart if one
        // trains 3 days a week and the other 2 - so the end date is walked forward day by day
        // over the real training days rather than derived from a flat calendar duration or an
        // averaged weekly cadence.
        //
        // Deliberately walks the same way GenerateSessionOccurrencesCommandHandler does
        // (step one day, compare DayOfWeek against each scheduled day) so the date this lands
        // on is the date of the sessionsth actual generated session, not an approximation of it.
        public static DateOnly ComputeEndDate(
            DateOnly startDate,
            int totalSessions,
            IReadOnlyCollection<DayOfWeek> trainingDays)
        {
            if (trainingDays is null || trainingDays.Count == 0)
                throw new GroupHasNoScheduleException();

            if (totalSessions <= 0)
                return startDate;

            var distinctDays = trainingDays.Distinct().ToHashSet();

            var counted = 0;
            var date = startDate;

            // The start date itself counts when it falls on a training day - a trainee assigned
            // on a day their group trains attends that same day.
            while (true)
            {
                if (distinctDays.Contains(date.DayOfWeek))
                {
                    counted++;
                    if (counted == totalSessions)
                        return date;
                }

                date = date.AddDays(1);
            }
        }

        /// <summary>
        /// Total sessions a subscription buys across its whole term - the frequency
        /// (DaysPerMonth) multiplied by the duration (NumberOfMonths). Used only for working
        /// out how far forward the term reaches; the per-enrollment attendance quota is a
        /// separate figure (see SubscriptionDetailsService.CalculateAllowedSessions).
        /// </summary>
        public static int CalculateTotalSessions(int daysPerMonth, int numberOfMonths)
            => daysPerMonth * Math.Max(numberOfMonths, 1);
    }
}
