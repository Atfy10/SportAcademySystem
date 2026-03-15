using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Views.EmployeeViews;

public class EmployeeWorkView
{
    public int Id { get; set; }

    public decimal Salary { get; set; }

    public DateTime HireDate { get; set; }

    public Position Position { get; set; }

    public string BranchName { get; set; } = null!;

    public string UserName { get; set; } = null!;

}
