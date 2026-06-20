using AutoMapper;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Mappings
{
    public class CoachMappingProfile : AutoMapper.Profile
    {
        public CoachMappingProfile()
        {
            CreateMap<Coach, CoachCardDto>()
                .ConstructUsing((src, _) => new CoachCardDto(
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
                ));

            CreateMap<Coach, CoachDetailsDto>()
                .ConstructUsing((src, _) => new CoachDetailsDto(
                    src.EmployeeId,
                    src.Employee.FirstName,
                    src.Employee.LastName,
                    src.Employee.Email.ToString(),
                    src.Employee.PhoneNumber,
                    src.Employee.Branch.Name,
                    src.Sport.Name,
                    src.SkillLevel.ToString(),
                    null,
                    src.TraineeGroups
                        .SelectMany(tg => tg.Enrollments)
                        .Count(e => e.IsActive && !e.IsDeleted),
                    src.Employee.HireDate,
                    src.Employee.IsWork,
                    src.Rate
                ));

            CreateMap<Coach, CoachSummaryDto>()
                .ConstructUsing(src => new CoachSummaryDto
                {
                    Id = src.EmployeeId,
                    Name = src.Employee.FirstName + " " + src.Employee.LastName,
                    SportName = src.Sport.Name,
                    BirthDate = src.Employee.BirthDate,
                    SkillLevel = src.SkillLevel.ToString()
                });

            CreateMap<Coach, CoachDropdownItemDto>()
                .ConstructUsing(src => new CoachDropdownItemDto
                {
                    Id = src.EmployeeId,
                    EmployeeFirstName = src.Employee.FirstName,
                    EmployeeLastName = src.Employee.LastName,
                    BranchId = src.Employee.BranchId,
                    BranchName = src.Employee.Branch.Name
                });
        }
    }
}
