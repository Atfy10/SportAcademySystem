using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.SportDtos
{
    public class SportDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
