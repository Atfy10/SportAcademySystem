using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.EmployeeDtos
{
    public record EmployeeDto(
        int Id,
        string FirstName,
        string LastName,
        string SSN,
        decimal Salary,
        Gender Gender,
        DateTime HireDate,
        string Address,
        string PhoneNumber,
        string? SecondPhoneNumber,
        Position Position,
        int BranchId,
        string AppUserId
    );
}
