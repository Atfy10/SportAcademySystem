using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.SportPriceDtos
{
	public class SportPriceDto
	{
		public int SportId { get; set; }
		public string SportName { get; set; } = string.Empty;

		public int BranchId { get; set; }
		public string BranchName { get; set; } = string.Empty;

		public int SubsTypeId { get; set; }
		public string SubscriptionTypeName { get; set; } = string.Empty;

		public decimal Price { get; set; }
	}

}
