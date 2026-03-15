using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Views.TraineeViews
{
    public class TraineeBasicView
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
}
