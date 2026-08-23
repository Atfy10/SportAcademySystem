namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class SubscriptionGroupSportMismatchException : Exception
    {
        public SubscriptionGroupSportMismatchException(int subscriptionDetailsId, int traineeGroupId)
            : base($"Subscription {subscriptionDetailsId} is for a different sport than trainee group " +
                   $"{traineeGroupId} teaches. An enrollment ties a subscription to a group for the same " +
                   "sport - pick a subscription and a group for the same sport.")
        {
        }
    }
}
