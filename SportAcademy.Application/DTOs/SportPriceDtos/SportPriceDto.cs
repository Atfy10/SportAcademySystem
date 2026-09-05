using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Domain.Enums;

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

		// Part of the price's identity, not a display detail: public and private training for
		// the same sport/branch/plan are separate rows, and the UI keys, edits, and deletes by it.
		// A PascalCase string ("Public"/"Private"), not the enum: serialized as an enum it would
		// arrive camelCase and silently fail to match the form's own option values, which is
		// exactly how a private price came to be displayed - and re-saved - as public.
		public string GroupType { get; init; } = nameof(TraineeGroupType.Public);

		public decimal Price { get; init; }
	}

}
