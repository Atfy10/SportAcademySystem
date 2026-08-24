namespace SportAcademy.Domain.Exceptions.PaymentTypeExceptions
{
    public class PaymentTypeInUseException : Exception
    {
        public PaymentTypeInUseException(int paymentTypeId)
            : base($"Payment type {paymentTypeId} is used by one or more recorded payments and cannot be " +
                   "deleted. Deactivate it instead to hide it from new payments while preserving history.")
        { }
    }
}
