using AutoMapper;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Mappings
{
    public class SubscriptionDetailsMappingProfile : AutoMapper.Profile
    {
        public SubscriptionDetailsMappingProfile()
        {
            CreateMap<SubscriptionDetails, SubscriptionDetailsDropdownDto>()
                .ConstructUsing(src => new SubscriptionDetailsDropdownDto(
                    src.Id,
                    src.SportPrice.SportSubscriptionType.SubscriptionType.Name
                ));
        }
    }
}
