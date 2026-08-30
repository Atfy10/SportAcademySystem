using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using System;

namespace SportAcademy.Domain.Exceptions.NationalityCategoryExceptions
{
    public class NationalityCategoryNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(NationalityCategory);

        public NationalityCategoryNotFoundException(string id) : base(_entity, id)
        {
        }
        public NationalityCategoryNotFoundException(string id, Exception innerException) : base(_entity, id, innerException)
        {
        }
    }
}
