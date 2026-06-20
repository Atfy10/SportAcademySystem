using SportAcademy.Application.Commands.SportPriceCommands.CreateSportPrice;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class SportPriceMappings
    {
        public static SportPrice ToSportPrice(this CreateSportPriceCommand cmd)
        {
            return SportPrice.Create(
                cmd.SportId,
                cmd.BranchId,
                cmd.SubsTypeId,
                cmd.Price);
        }

        public static SportPriceBranchDto ToBranchDto(this SportPrice sportPrice)
        {
            return new SportPriceBranchDto
            {
                BranchName = sportPrice.Branch.Name,
                SportName = sportPrice.SportSubscriptionType.Sport.Name,
                Price = sportPrice.Price
            };
        }

        public static SportPriceDto ToDto(this SportPrice sportPrice)
        {
            return new SportPriceDto
            {
                SportId = sportPrice.SportId,
                SportName = sportPrice.SportSubscriptionType.Sport.Name,
                BranchId = sportPrice.BranchId,
                BranchName = sportPrice.Branch.Name,
                SubsTypeId = sportPrice.SubsTypeId,
                SubscriptionTypeName = sportPrice.SportSubscriptionType.SubscriptionType.Name.ToString(),
                Price = sportPrice.Price
            };
        }
    }
}
