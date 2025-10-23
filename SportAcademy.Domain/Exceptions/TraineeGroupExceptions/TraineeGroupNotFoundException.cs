using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.TraineeGroupExceptions
{
    public class TraineeGroupNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(TraineeGroup);

        public TraineeGroupNotFoundException(string id) : base(_entity, id) { }

        public TraineeGroupNotFoundException(string id, Exception innerException)
            : base(_entity, id, innerException) { }

    }
}
