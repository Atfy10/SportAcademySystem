using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.SportPriceDtos
{
	public record SportPriceDto
	{
		public int SportId { get; init; }
		public string SportName { get; init; } = string.Empty;

		public int BranchId { get; init; }
		public string BranchName { get; init; } = string.Empty;

		public int SubsTypeId { get; init; }
		public string SubscriptionTypeName { get; init; } = string.Empty;

		public decimal Price { get; init; }
	}

}
