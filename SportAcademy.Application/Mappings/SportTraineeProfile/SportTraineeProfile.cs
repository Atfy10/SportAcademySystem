using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee;
using SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.SportTraineeProfile
{
	public class SportTraineeProfile : AutoMapper.Profile
	{
		public SportTraineeProfile()
		{
			CreateMap<CreateSportTraineeCommand, SportTrainee>().ReverseMap();
			CreateMap<UpdateSportTraineeCommand, SportTrainee>();
		}
	}
}
