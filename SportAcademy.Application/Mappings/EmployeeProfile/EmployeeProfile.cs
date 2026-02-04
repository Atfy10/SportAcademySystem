using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Mappings.EmployeeProfile
{
    public class EmployeeMappingProfile : AutoMapper.Profile
    {
        public EmployeeMappingProfile()
        {
            CreateMap<Employee, EmployeeDto>().ReverseMap();

            CreateMap<CreateEmployeeCommand, Employee>()
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src =>
                    Address.Create(src.Street, src.City)))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src =>
                    Email.Create(src.Email)));

            CreateMap<UpdateEmployeeCommand, Employee>();
        }
    }
}
