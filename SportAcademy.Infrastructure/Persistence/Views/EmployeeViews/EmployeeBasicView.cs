using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.Views.Interfaces;

namespace SportAcademy.Infrastructure.Persistence.Views.EmployeeViews;

public class EmployeeBasicView : IModelView
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string SSN { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public Gender Gender { get; set; }

    public string City { get; set; } = null!;

    public string? SecondPhoneNumber { get; set; }

}
