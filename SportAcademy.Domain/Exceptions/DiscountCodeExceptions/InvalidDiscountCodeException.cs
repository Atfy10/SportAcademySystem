namespace SportAcademy.Domain.Exceptions.DiscountCodeExceptions
{
    // Thrown for a code that's unknown, inactive, or past its expiry - deliberately one message
    // for all three (never reveal to the caller *why* a code was rejected beyond "invalid").
    public class InvalidDiscountCodeException : Exception
    {
        public InvalidDiscountCodeException(string code)
            : base($"Discount code \"{code}\" is invalid, inactive, or expired.")
        { }
    }
}
