namespace SportAcademy.Domain.Entities
{
    public class NationalityCategory
    {
        public int Id { get; private set; }
        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;

        public virtual ICollection<Trainee> Trainees { get; set; } = [];

        private NationalityCategory() { }

        public NationalityCategory(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        public NationalityCategory(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public static NationalityCategory Create(string code, string name) => new(code, name);
    }
}
