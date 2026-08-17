using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written replacement for CreateMap<UpdateEmployeeCommand, Employee>() /
    // CreateMap<Employee, EmployeeDto>() in EmployeeProfile.cs - used only by
    // UpdateEmployeeCommandHandler. Every update field is optional and only overwrites the
    // entity when the command actually carries it.
    public static class EmployeeMapper
    {
        public static EmployeeDto ToDto(Employee employee) => new(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.SSN,
            employee.Salary,
            employee.Gender,
            employee.HireDate,
            employee.Address.ToString(),
            employee.PhoneNumber,
            employee.SecondPhoneNumber,
            employee.Position,
            employee.BranchId,
            employee.AppUserId ?? Guid.Empty
        );

        public static void ApplyUpdate(Employee employee, UpdateEmployeeCommand cmd)
        {
            if (cmd.FirstName != null) employee.FirstName = cmd.FirstName;
            if (cmd.LastName != null) employee.LastName = cmd.LastName;
            if (cmd.Salary.HasValue) employee.Salary = cmd.Salary.Value;
            if (cmd.PhoneNumber != null) employee.PhoneNumber = cmd.PhoneNumber;
            if (cmd.SecondPhoneNumber != null) employee.SecondPhoneNumber = cmd.SecondPhoneNumber;
            if (cmd.Position.HasValue) employee.Position = cmd.Position.Value;
            if (cmd.BranchId.HasValue) employee.BranchId = cmd.BranchId.Value;

            if (cmd.Street != null || cmd.City != null)
            {
                var street = cmd.Street ?? employee.Address.Street;
                var city = cmd.City ?? employee.Address.City;
                employee.Address = Address.Create(street, city);
            }
        }
    }
}
