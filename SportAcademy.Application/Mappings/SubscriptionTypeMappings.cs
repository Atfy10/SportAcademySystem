using SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType;
using SportAcademy.Application.DTOs.SubscriptionTypeDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class SubscriptionTypeMappings
    {
        public static SubscriptionTypeDto ToDto(this SubscriptionType entity)
        {
            return new SubscriptionTypeDto
            {
                Id = entity.Id,
                Name = entity.Name.ToString(),
                DaysPerMonth = entity.DaysPerMonth,
                NumberOfMonths = entity.NumberOfMonths,
                IsActive = entity.IsActive,
                IsOffer = entity.IsOffer,
                Sports = entity.Sports?
                    .Where(sst => sst.Sport != null)
                    .Select(sst => sst.Sport.Name)
                    .ToList() ?? [],
                SportIds = entity.Sports?
                    .Select(sst => sst.SportId)
                    .ToList() ?? []
            };
        }

        public static SubscriptionType ToEntity(this CreateSubscriptionTypeCommand cmd)
        {
            return new SubscriptionType
            {
                Name = (Domain.Enums.SubType)System.Enum.Parse(typeof(Domain.Enums.SubType), cmd.Name),
                DaysPerMonth = cmd.DaysPerMonth,
                NumberOfMonths = cmd.NumberOfMonths,
                IsActive = cmd.IsActive,
                IsOffer = cmd.IsOffer,
                Sports = cmd.SportIds.Select(id => new SportSubscriptionType
                {
                    SportId = id
                }).ToList()
            };
        }
    }
}