using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.ExcuseRequestExceptions
{
    public class ExcuseRequestNotFoundException : IdNotFoundException
    {
        static readonly string _entity = nameof(ExcuseRequest);

        public ExcuseRequestNotFoundException(string id) : base(_entity, id) { }
    }
}
