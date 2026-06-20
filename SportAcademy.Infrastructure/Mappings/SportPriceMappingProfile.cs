using AutoMapper;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Mappings
{
    public class SportPriceMappingProfile : AutoMapper.Profile
    {
        public SportPriceMappingProfile()
        {
            CreateMap<SportPrice, SportPriceBranchDto>()
                .ConstructUsing((src, _) => new SportPriceBranchDto
                {
                    BranchName = src.Branch.Name,
                    SportName = src.SportSubscriptionType.Sport.Name,
                    Price = src.Price
                });

            CreateMap<SportPrice, SportPriceDto>()
                .ConstructUsing((src, _) => new SportPriceDto
                {
                    SportId = src.SportId,
                    SportName = src.SportSubscriptionType.Sport.Name,
                    BranchId = src.BranchId,
                    BranchName = src.Branch.Name,
                    SubsTypeId = src.SubsTypeId,
                    SubscriptionTypeName = src.SportSubscriptionType.SubscriptionType.Name,
                    Price = src.Price
                });
        }
    }
}
