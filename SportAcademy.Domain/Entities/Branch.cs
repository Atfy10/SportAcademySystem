using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Entities
{
    internal class Branch
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public required string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public required string CoX { get; set; }
        public required string CoY { get; set; }
    }
}
