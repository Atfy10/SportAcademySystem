using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.BranchDtos
{
	public class BranchDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = null!;
		public string City { get; set; } = null!;
		public string Country { get; set; } = null!;
		public string PhoneNumber { get; set; } = null!;
		public string? Email { get; set; }
		public string CoX { get; set; } = null!;
		public string CoY { get; set; } = null!;
		public bool IsActive { get; set; }
	}

}
