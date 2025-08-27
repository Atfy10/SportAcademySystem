using Microsoft.AspNetCore.Identity;

namespace SportAcademy.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public bool IsBanned { get; set; }

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;
        //Add Nav Profile
        public virtual Profile Profile { get; set; } = null!;
    }
}
