using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.EmployeeDtos
{
    public class EmployeeWorkDto
    {
        public int Id { get; set; }

        public decimal Salary { get; set; }

        public DateTime HireDate { get; set; }

        public Position Position { get; set; }

        public string BranchName { get; set; } = null!;

        public string UserName { get; set; } = null!;
    }
}
