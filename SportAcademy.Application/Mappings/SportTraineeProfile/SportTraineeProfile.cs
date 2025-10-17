using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Application.Commands.SportTraineeCommands.CreateSportTrainee;
using SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings.SportTraineeProfile
{
	public class SportTraineeProfile : AutoMapper.Profile
	{
		public SportTraineeProfile()
		{
			CreateMap<CreateSportTraineeCommand, SportTrainee>()
				.ForMember(dest => dest.SkillLevel, opt => opt.MapFrom(src => Enum.Parse<SkillLevel>(src.SkillLevel)));

			CreateMap<UpdateSportTraineeCommand, SportTrainee>()
				.ForMember(dest => dest.SkillLevel, opt => opt.MapFrom(src => Enum.Parse<SkillLevel>(src.SkillLevel)));

			CreateMap<SportTrainee, SportTraineeDto>()
				.ForMember(dest => dest.SkillLevel, opt => opt.MapFrom(src => src.SkillLevel.ToString()))
				.ForMember(dest => dest.SportName, opt => opt.MapFrom(src => src.Sport.Name))
				.ForMember(dest => dest.TraineeName, opt => opt.MapFrom(src => src.Trainee.FirstName + " " + src.Trainee.LastName));
		}
	}
}
