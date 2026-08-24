namespace SportAcademy.Domain.Exceptions.PaymentTypeExceptions
{
    public class NoDefaultPaymentTypeException : Exception
    {
        public NoDefaultPaymentTypeException()
            : base("No payment types are configured for this tenant yet. Set one up under " +
                   "Payment Types before recording or marking payments as paid.")
        { }
    }
}
