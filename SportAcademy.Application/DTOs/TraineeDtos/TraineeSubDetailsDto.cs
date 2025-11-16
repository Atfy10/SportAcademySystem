using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.TraineeDtos
{
    public record TraineeSubDetailsDto
    {
        public int Id { get; init; }
        public string FullName { get; init; } = null!;
        public string PhoneNumber { get; init; } = null!;
    }
}
