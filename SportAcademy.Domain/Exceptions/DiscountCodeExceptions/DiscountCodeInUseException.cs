namespace SportAcademy.Domain.Exceptions.DiscountCodeExceptions
{
    public class DiscountCodeInUseException : Exception
    {
        public DiscountCodeInUseException(int discountCodeId)
            : base($"Discount code {discountCodeId} has been used on one or more invoices and cannot be " +
                   "deleted. Deactivate it instead to stop new redemptions while preserving history.")
        { }
    }
}
