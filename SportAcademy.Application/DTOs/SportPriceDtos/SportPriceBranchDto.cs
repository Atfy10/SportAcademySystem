using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.SportPriceDtos
{
    public record SportPriceBranchDto
    {
        public string BranchName { get; init; } = null!;
        public string SportName { get; init; } = null!;
        public decimal Price { get; init; }
    }
}
