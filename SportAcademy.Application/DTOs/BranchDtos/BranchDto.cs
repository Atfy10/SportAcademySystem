using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.BranchDtos
{
	public record BranchDto
	{
		public int Id { get; init; }
		public string Name { get; init; } = null!;
		public string City { get; init; } = null!;
		public string Country { get; init; } = null!;
		public string PhoneNumber { get; init; } = null!;
		public string? Email { get;	init; }
		public string CoX { get; init; } = null!;
		public string CoY { get; init; } = null!;
		public bool IsActive { get; init; }
	}

}
