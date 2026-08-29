namespace SportAcademy.Domain.Exceptions.BaseExceptions
{
    public class IdNotFoundException : Exception
    {
        private const string _message = "{entity} with ID: {id} not found.";

        public IdNotFoundException(string entity, object id)
            : base(FormatMessage(entity, id))
        {
            Entity = entity;
            Id = id;
        }

        public IdNotFoundException(string entity, object id, Exception inner)
            : base(FormatMessage(entity, id), inner)
        {
            Entity = entity;
            Id = id;
        }

        /// <summary>
        /// The entity's CLR type name, e.g. <c>Trainee</c>.
        /// </summary>
        /// <remarks>
        /// Exposed separately from <see cref="Exception.Message"/> so the pipeline can log it as a
        /// queryable structured property and translate it via the <c>entity.*</c> catalog, instead
        /// of shipping the assembled English sentence - which leaks a type name and a row id - to
        /// the user.
        /// </remarks>
        public string Entity { get; }

        /// <summary>The identifier that was not found. Log-only: never shown to a user.</summary>
        public object Id { get; }

        private static string FormatMessage(string entity, object id) =>
            _message.Replace("{entity}", entity).Replace("{id}", id.ToString() ?? "");
    }
}
