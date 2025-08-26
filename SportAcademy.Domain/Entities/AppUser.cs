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
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string SSN { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsBanned { get; set; }
    }
}
