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
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src =>
                    Address.Create(src.Street, src.City)))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src =>
                    Email.Create(src.Email)))
                .ForMember(dest => dest.SecondPhoneNumber,
                    opt => opt.MapFrom(src =>
                    src.SecondNumber))
                .ForMember(dest => dest.Nationality,
                    opt => opt.MapFrom(src =>
                    Enum.Parse<Nationality>(src.Nationality)));

            CreateMap<CreateEmployeeDto, Employee>();

            CreateMap<UpdateEmployeeCommand, Employee>()
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src =>
                    Address.Create(src.Street, src.City)))
;
        }
    }
}
