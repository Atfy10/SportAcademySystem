using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.AttendanceExceptions
{
    public class AttendanceNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(Attendance);

        public AttendanceNotFoundException(string id) : base(_entity, id) { }

        public AttendanceNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }

    }
}
