using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class SubscriptionGroupTypeMismatchException : Exception
    {
        public SubscriptionGroupTypeMismatchException(
            int subscriptionDetailsId,
            int traineeGroupId,
            TraineeGroupType subscriptionGroupType,
            TraineeGroupType groupType)
            : base($"Subscription {subscriptionDetailsId} was priced for a {subscriptionGroupType} group, " +
                   $"but trainee group {traineeGroupId} is {groupType}. Public and private training are " +
                   "priced differently - pick a group matching the subscription, or create a new " +
                   "subscription for the other type.")
        {
        }
    }
}
