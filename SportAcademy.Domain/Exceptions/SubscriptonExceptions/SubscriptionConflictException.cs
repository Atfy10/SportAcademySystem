using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.SubscriptonExceptions
{
    // A trainee can hold only one subscription per sport at a time, and a new one has to start
    // after the latest one they have in that sport ends - never before or during it. Localizable
    // (was a plain Exception, which reached the user as a generic "something went wrong") and
    // tells them the first date that would be accepted.
    public class SubscriptionConflictException : LocalizableException
    {
        public SubscriptionConflictException(DateOnly existingEndDate)
            : base(
                "errors.subscription.sameSportConflict",
                $"This trainee already has a subscription for this sport until {existingEndDate:yyyy-MM-dd}. A new one can start on {existingEndDate.AddDays(1):yyyy-MM-dd} at the earliest.",
                existingEndDate.ToString("yyyy-MM-dd"),
                existingEndDate.AddDays(1).ToString("yyyy-MM-dd"))
        {
        }
    }
}
