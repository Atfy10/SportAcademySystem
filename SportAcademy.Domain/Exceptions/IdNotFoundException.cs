using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
    public class IdNotFoundException : Exception
    {
        private const string _message = "{entity} with ID: {id} not found.";
        public IdNotFoundException(string entity, string id)
            : base($"{FormatMessage(entity, id)}")
        {
        }
        public IdNotFoundException(string entity, string id, Exception inner)
            : base(FormatMessage(entity, id), inner)
        {
        }

        private static string FormatMessage(string entity, string id) =>
            _message.Replace("{entity}", entity).Replace("{id}", id);
    }
}
