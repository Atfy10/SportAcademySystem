namespace SportAcademy.Domain.Exceptions.UserExceptions
{
    public class EmailSyntaxIncorrectException : Exception
    {
        private readonly static string _message = "The provided email has incorrect syntax.";

        public EmailSyntaxIncorrectException() : base(_message) { }
        public EmailSyntaxIncorrectException(Exception innerException)
            : base(_message, innerException) { }
    }
}
