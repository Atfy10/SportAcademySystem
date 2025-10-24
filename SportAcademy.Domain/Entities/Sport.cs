using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Sport
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public SportCategory Category { get; set; }
        public bool IsRequireHealthTest { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Coach> Coaches { get; set; } = [];
        public virtual ICollection<SportSubscriptionType> SubscriptionTypes { get; set; } = [];
        public virtual ICollection<SportBranch> Branches { get; set; } = [];
        public virtual ICollection<SportTrainee> Trainees { get; set; } = [];
	}
}
