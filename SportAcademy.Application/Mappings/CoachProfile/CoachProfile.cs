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
                .ConstructUsing(src => new CoachCardDto(
                    src.EmployeeId,
                    src.Employee.FirstName,
                    src.Employee.LastName,
                    src.Employee.Position.ToString(),
                    src.Employee.Branch.Name,
                    src.Employee.Email.ToString(),
                    src.Employee.IsWork,
                    src.Employee.PhoneNumber,
                    src.Employee.Address.ToString(),
                    src.Employee.HireDate,
                    src.TraineeGroups
                        .SelectMany(tg => tg.Enrollments)
                        .Count(e => e.IsActive && !e.IsDeleted),
                    src.SkillLevel,
                    src.Sport.Name
                ))
                .ReverseMap();

            CreateMap<CreateCoachCommand, Coach>();

            CreateMap<CreateCoachWithEmployeeCommand, Coach>()
                .ForMember(
                    dest => dest.Employee,
                    opt => opt.Ignore()
                );

            CreateMap<Coach, CoachSummaryDto>().ReverseMap();

            CreateMap<Coach, CoachDetailsDto>()
                .ConstructUsing(src => new CoachDetailsDto
                (
                    src.EmployeeId,
                    src.Employee.FirstName,
                    src.Employee.LastName,
                    src.Employee.Email.ToString(),
                    src.Employee.PhoneNumber,
                    src.Employee.Branch.Name,
                    src.Sport.Name,
                    src.SkillLevel.ToString(),
                    null, // Certifications not implemented yet
                    src.TraineeGroups
                        .SelectMany(tg => tg.Enrollments)
                        .Count(e => e.IsActive && !e.IsDeleted),
                    src.Employee.HireDate,
                    src.Employee.IsWork,
                    src.Rate
                ));

            // .ForMember() here, not .ConstructUsing() - ConstructUsing opts a mapping out of
            // AutoMapper's LINQ expression-tree translation, which ProjectTo relies on to turn
            // this into a SQL projection. Left as ConstructUsing, this was throwing
            // InvalidCastException at runtime on the coach dropdown endpoint.
            CreateMap<Coach, CoachDropdownItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.EmployeeFirstName, opt => opt.MapFrom(src => src.Employee.FirstName))
                .ForMember(dest => dest.EmployeeLastName, opt => opt.MapFrom(src => src.Employee.LastName))
                .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.Employee.BranchId))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Employee.Branch.Name));
        }
    }
}
