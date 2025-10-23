using SportAcademy.Domain.Exceptions.BaseExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions
{
    public class SessionOccurenceNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(Entities.SessionOccurrence);

        public SessionOccurenceNotFoundException(string id) : base(_entity, id) { }

        public SessionOccurenceNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }

    }
}
