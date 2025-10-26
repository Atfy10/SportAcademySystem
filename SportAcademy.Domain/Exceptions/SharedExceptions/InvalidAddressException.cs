namespace SportAcademy.Domain.Exceptions.SharedExceptions
{
    public class InvalidAddressException : Exception
    {
        private readonly static string _message = "The provided address is invalid.";

        public InvalidAddressException() : base(_message) { }
        public InvalidAddressException(Exception innerException)
            : base(_message, innerException) { }
    }
}
