using System;
using System.Collections.Generic;

namespace SportAcademy.Domain.Entities
{
    public class Feature
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ClientFeature> ClientFeatures { get; set; } = [];
    }
}
