using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Sport
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Category { get; set; }
        public bool IsRequireHealthTest { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Coach> Coaches { get; set; } = [];
        public virtual ICollection<SportSubscriptionType> SubscriptionTypes { get; set; } = [];
        public virtual ICollection<SportBranch> Branches { get; set; } = [];
        public virtual ICollection<SportTrainee> Trainees { get; set; } = [];
        public virtual ICollection<SportPrice> Prices { get; set; } = [];
    }
}
