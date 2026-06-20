using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Application.Mappings.EmployeeProfile
{
    public class EmployeeMappingProfile : AutoMapper.Profile
    {
        public EmployeeMappingProfile()
        {
            CreateMap<Employee, EmployeeDto>().ReverseMap();

            CreateMap<Employee, EmployeeCardDto>()
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Branch.Name))
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src => src.Address.ToString()))
                .ReverseMap();

            CreateMap<CreateEmployeeCommand, Employee>()
                .ConstructUsing((src, _) =>
                {
                    var address = Address.Create(src.Street, src.City);
                    var email = Email.Create(src.Email);
                    var nationality = Enum.Parse<Nationality>(src.Nationality);

                    var data = new PersonData(
                        src.FirstName,
                        src.LastName,
                        src.SSN,
                        email,
                        src.BirthDate,
                        src.Gender,
                        nationality,
                        address,
                        src.PhoneNumber,
                        src.SecondNumber);

                    return Employee.Create(data, src.Salary, src.Position, src.BranchId);
                });

            CreateMap<CreateEmployeeDto, Employee>()
                .ConstructUsing((src, _) =>
                {
                    var data = new PersonData(
                        src.FirstName,
                        src.LastName,
                        src.SSN,
                        Email.Create(src.FirstName + "@academy.com"),
                        src.BirthDate,
                        src.Gender,
                        Nationality.Kuwaiti,
                        Address.Create(src.Address, src.Address),
                        src.PhoneNumber,
                        src.SecondNumber);

                    return Employee.Create(data, src.Salary, src.Position, src.BranchId);
                })
                .ForMember(dest => dest.Address, opt => opt.Ignore());

            CreateMap<UpdateEmployeeCommand, Employee>()
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src =>
                    Address.Create(src.Street, src.City)))
;
        }
    }
}
