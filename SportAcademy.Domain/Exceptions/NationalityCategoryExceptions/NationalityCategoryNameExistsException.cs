using System;

namespace SportAcademy.Domain.Exceptions.NationalityCategoryExceptions
{
    public class NationalityCategoryNameExistsException : Exception
    {
        static readonly string _message = "A nationality category with the same name already exists. Please choose a different name.";
        public NationalityCategoryNameExistsException() : base(_message)
        {
        }
        public NationalityCategoryNameExistsException(Exception innerException) : base(_message, innerException)
        {
        }
    }
}
