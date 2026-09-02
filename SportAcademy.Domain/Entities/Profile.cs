using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    public class Profile
    {
        public required Guid AppUserId { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool HasCompletedOnboarding { get; set; }
        public string? PreferredLanguage { get; set; }

        // Navigation Property
        public virtual AppUser User { get; set; } = null!;
    }
}
