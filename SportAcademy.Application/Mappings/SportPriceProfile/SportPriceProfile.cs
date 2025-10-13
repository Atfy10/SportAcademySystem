using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Application.Commands.SportPriceCommands.CreateSportPrice;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.SportPriceProfile
{
	public class SportPriceProfile : AutoMapper.Profile
	{
		public SportPriceProfile()
		{
			CreateMap<SportPrice, CreateSportPriceCommand>().ReverseMap();
			CreateMap<SportPrice, SportPriceDto>()
				.ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport.Name))
				.ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
				.ForMember(dest => dest.SubscriptionTypeName, opt => opt.MapFrom(src => src.SubscriptionType.Name.ToString()));
		}
	}
}