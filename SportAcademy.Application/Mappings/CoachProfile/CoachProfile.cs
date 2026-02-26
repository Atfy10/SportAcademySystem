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
            CreateMap<Coach, CoachCardDto>()
                .ForMember(dest => dest.FirstName,
                    opt => opt.MapFrom(src => src.Employee.FirstName))
                .ForMember(dest => dest.LastName,
                    opt => opt.MapFrom(src => src.Employee.LastName))
                .ForMember(dest => dest.Position,
                    opt => opt.MapFrom(src => src.Employee.Position.ToString()))
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Employee.Branch.Name))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.Employee.Email))
                .ForMember(dest => dest.IsWork,
                    opt => opt.MapFrom(src => src.Employee.IsWork))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.Employee.PhoneNumber))
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src => src.Employee.Address.ToString()))
                .ForMember(dest => dest.HireDate,
                    opt => opt.MapFrom(src => src.Employee.HireDate))
                .ForMember(dest => dest.TotalTrainees,
                    opt => opt.MapFrom(src => src.TraineeGroups.Sum(tg => tg.MaximumCapacity)))
                .ForMember(dest => dest.Sport,
                    opt => opt.MapFrom(src => src.Sport))
                .ReverseMap();

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
