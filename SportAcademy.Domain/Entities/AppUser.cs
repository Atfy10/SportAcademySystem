using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SportAcademy.Domain.Entities
{
    internal class AppUser : IdentityUser
    {
        public bool IsBanned { get; set; }

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;
    }
}
