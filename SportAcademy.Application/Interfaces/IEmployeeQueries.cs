using SportAcademy.Application.DTOs.EmployeeDtos;

namespace SportAcademy.Application.Interfaces
{
    public interface IEmployeeQueries
    {
        Task<IReadOnlyList<EmployeeBasicDto>> GetEmployeesAsync();

        Task<EmployeeBasicDto?> GetEmployeeByIdAsync(int employeeId);

        Task<IReadOnlyList<EmployeeWorkDto>> GetEmployeesWorkAsync();
    }
}
