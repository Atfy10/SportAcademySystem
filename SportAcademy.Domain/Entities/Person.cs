using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Domain.Entities
{
    public abstract class Person : IAuditableEntity, ISoftDeletable
    {
        protected Person(PersonData data)
        {
            FirstName = data.FirstName.Trim();
            LastName = data.LastName.Trim();
            SSN = data.SSN;
            Email = data.Email;
            BirthDate = data.BirthDate;
            Gender = data.Gender;
            Nationality = data.Nationality;
            Address = data.Address;
            PhoneNumber = data.PhoneNumber;
            SecondPhoneNumber = data.SecondPhoneNumber;
        }

        protected Person() { }

        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public string SSN { get; private set; } = null!;
        public Email Email { get; private set; } = null!;
        public DateOnly BirthDate { get; private set; }
        public Gender Gender { get; private set; }
        public Nationality Nationality { get; private set; }
        public Address Address { get; private set; } = null!;
        public string PhoneNumber { get; private set; } = null!;
        public string? SecondPhoneNumber { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public int CalculateAge()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - BirthDate.Year;
            if (BirthDate > today.AddYears(-age))
                age--;
            return age;
        }

        public bool IsAdult => CalculateAge() >= 18;

        public static bool IsSsnValid(string ssn, DateOnly birthDate)
        {
            if (ssn.Length != 12 || !ssn.All(char.IsDigit))
                return false;

            var prefix = birthDate.Year > 1999 ? "3" : "2";
            var expectedStart = $"{prefix}{birthDate:yyMMdd}";

            return ssn.StartsWith(expectedStart);
        }
    }
}
