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
        }
    }
}
