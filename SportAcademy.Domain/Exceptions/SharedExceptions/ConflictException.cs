using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.SharedExceptions
{
    public class ConflictException : Exception
    {
        private static readonly string _message = "Entity {Entity1} is already assigned to {Entity2} entity.";

        public ConflictException(string firstEntity, string secondEntity)
            : base(FormatMessage(firstEntity, secondEntity))
        {
        }

        public ConflictException(string firstEntity, string secondEntity, Exception innerException)
            : base(FormatMessage(firstEntity, secondEntity), innerException)
        {
        }

        private static string FormatMessage(string firstEntity, string secondEntity)
        {
            return _message.Replace("{Entity1}", firstEntity)
                           .Replace("{Entity2}", secondEntity);
        }
    }
}
