using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Views.AdminViews
{
    public class AdminBasicViews
    {
        public string UserName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string SSN { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }
        public string City { get; set; } = null!;
        public string? SecondPhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
