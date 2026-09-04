namespace SportAcademy.Domain.Exceptions.SubscriptionDiscountRequestExceptions
{
    public class InvalidSubscriptionDiscountRequestTransitionException : Exception
    {
        public InvalidSubscriptionDiscountRequestTransitionException(int id, string currentStatus, string attemptedAction)
            : base($"Subscription discount request {id} is {currentStatus} and cannot be {attemptedAction}.")
        { }
    }
}
