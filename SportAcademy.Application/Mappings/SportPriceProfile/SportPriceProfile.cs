using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Application.Commands.SportPriceCommands.CreateSportPrice;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.SportPriceProfile
{
	public class SportPriceProfile : AutoMapper.Profile
	{
		public SportPriceProfile()
		{
			CreateMap<SportPrice, CreateSportPriceCommand>().ReverseMap();
		}
	}
}