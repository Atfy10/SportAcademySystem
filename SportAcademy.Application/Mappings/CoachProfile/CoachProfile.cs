using SportAcademy.Application.Commands.CoachCommands.CreateCoach;
using SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.CoachProfile
{
    public class CoachProfile : AutoMapper.Profile
    {
        public CoachProfile()
        {
            CreateMap<CreateCoachCommand, Coach>();

            CreateMap<CreateCoachWithEmployeeCommand, Coach>()
                .ForMember(
                    dest => dest.Employee,
                    opt => opt.Ignore()
                );

            CreateMap<Coach, CoachSummaryDto>().ReverseMap();
        }
    }
}
