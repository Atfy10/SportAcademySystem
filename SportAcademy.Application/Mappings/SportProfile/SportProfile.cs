using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Application.Commands.SportCommands.CreateSport;
using SportAcademy.Application.Commands.SportCommands.UpdateSport;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings.SportProfile
{
    public class SportProfile : AutoMapper.Profile
    {
        public SportProfile()
        {
            CreateMap<Sport, SportDto>()
                .ReverseMap()
				.ForMember(dest => dest.Coaches, opt => opt.Ignore())
                .ForMember(dest => dest.SubscriptionTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Branches, opt => opt.Ignore())
                .ForMember(dest => dest.Trainees, opt => opt.Ignore())
                .ForMember(dest => dest.Prices, opt => opt.Ignore());

			CreateMap<CreateSportCommand, Sport>()
			    .ForMember(dest => dest.Coaches, opt => opt.Ignore())
			    .ForMember(dest => dest.SubscriptionTypes, opt => opt.Ignore())
			    .ForMember(dest => dest.Branches, opt => opt.Ignore())
			    .ForMember(dest => dest.Trainees, opt => opt.Ignore())
			    .ForMember(dest => dest.Prices, opt => opt.Ignore());

            CreateMap<UpdateSportCommand, Sport>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
		}
	}
}
