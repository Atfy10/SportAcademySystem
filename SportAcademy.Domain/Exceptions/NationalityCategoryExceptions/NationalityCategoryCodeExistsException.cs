using System;

namespace SportAcademy.Domain.Exceptions.NationalityCategoryExceptions
{
    public class NationalityCategoryCodeExistsException : Exception
    {
        static readonly string _message = "A nationality category with the same code already exists. Please choose a different code.";
        public NationalityCategoryCodeExistsException() : base(_message)
        {
        }
        public NationalityCategoryCodeExistsException(Exception innerException) : base(_message, innerException)
        {
        }
    }
}
