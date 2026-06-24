using System;

namespace SportAcademy.Domain.Entities
{
    public class ClientFeature
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid FeatureId { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Feature Feature { get; set; } = null!;
    }
}
