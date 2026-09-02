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
            // EmployeeDto is an immutable record (positional constructor, no parameterless
            // ctor). .ForMember() forces AutoMapper into a member-init strategy that requires
            // a parameterless constructor + settable properties - it doesn't have either, so
            // that throws "needs to have a constructor with 0 args or only optional args" at
            // map time. .ForCtorParam() keeps AutoMapper on the constructor-mapping path that
            // records actually need. ReverseMap() is dropped: EmployeeDto -> Employee has no
            // caller (Employee mutation goes through CreateEmployeeDto/UpdateEmployeeCommand
            // via their own maps below / Mappings/Manual/EmployeeMapper.cs), and it would be
            // broken anyway since Employee.Address is a value object, not the two loose
            // Street/City strings this DTO carries.
            CreateMap<Employee, EmployeeDto>()
                .ForCtorParam(nameof(EmployeeDto.Street), opt => opt.MapFrom(src => src.Address.Street))
                .ForCtorParam(nameof(EmployeeDto.City), opt => opt.MapFrom(src => src.Address.City))
                .ForCtorParam(nameof(EmployeeDto.BranchName), opt => opt.MapFrom(src => src.Branch.Name))
                .ForCtorParam(nameof(EmployeeDto.Email), opt => opt.MapFrom(src => src.Email.Value));

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
                    Enum.Parse<Nationality>(src.Nationality)))
                // CreateEmployeeCommand has no HireDate field (it's not user-entered) -
                // without this, AutoMapper leaves it unmapped and it defaults to
                // DateTime.MinValue (0001-01-01), which is the "joined date" bug.
                .ForMember(dest => dest.HireDate,
                    opt => opt.MapFrom(src => DateTime.Now));

            // No ForMember overrides here previously - meant Address (string -> value object)
            // failed to map at all, and HireDate/Nationality/Email were silently left at CLR
            // defaults (0001-01-01, invalid enum 0, null respectively - the latter causing a
            // latent NullReferenceException in CoachCardDto's Email.ToString()). Same fix shape
            // as the sibling CreateEmployeeCommand mapping above; CreateEmployeeDto.Address is a
            // single free-text field rather than Street/City, so it maps to Address.Create with
            // an empty city.
            CreateMap<CreateEmployeeDto, Employee>()
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src =>
                    Address.Create(src.Address, string.Empty)))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src =>
                    Email.Create(src.Email)))
                .ForMember(dest => dest.SecondPhoneNumber,
                    opt => opt.MapFrom(src =>
                    src.SecondNumber))
                .ForMember(dest => dest.Nationality,
                    opt => opt.MapFrom(src =>
                    Enum.Parse<Nationality>(src.Nationality)))
                .ForMember(dest => dest.HireDate,
                    opt => opt.MapFrom(src => DateTime.Now));

            // UpdateEmployeeCommand -> Employee is no longer an AutoMapper mapping -
            // UpdateEmployeeCommandHandler uses Mappings/Manual/EmployeeMapper.cs instead
            // (partial update: fields are optional and only applied when present).
        }
    }
}
