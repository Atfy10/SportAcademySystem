using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Sport
    {
        private Sport() { }

        private Sport(string name, string? description, SportCategory category, bool isRequireHealthTest)
        {
            Name = name;
            Description = description;
            Category = category;
            IsRequireHealthTest = isRequireHealthTest;
        }

        public int Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public SportCategory Category { get; private set; }
        public bool IsRequireHealthTest { get; private set; } = true;

        public virtual ICollection<Coach> Coaches { get; private set; } = [];
        public virtual ICollection<SportSubscriptionType> SubscriptionTypes { get; private set; } = [];
        public virtual ICollection<SportBranch> Branches { get; private set; } = [];
        public virtual ICollection<SportTrainee> Trainees { get; private set; } = [];

        public static Sport Create(string name, string? description, SportCategory category, bool isRequireHealthTest)
        {
            return new Sport(name, description, category, isRequireHealthTest);
        }

        public void Update(string name, string? description, SportCategory category, bool isRequireHealthTest)
        {
            Name = name;
            Description = description;
            Category = category;
            IsRequireHealthTest = isRequireHealthTest;
        }
    }
}
