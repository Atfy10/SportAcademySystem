using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Domain.Entities
{
    public class Person : IAuditableEntity, ISoftDeletable
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string SSN { get; set; }
        public Email Email { get; set; } = null!;
        public DateOnly BirthDate { get; set; }
        public Gender Gender { get; set; }
        public Nationality Nationality { get; set; }
        public Address Address { get; set; } = null!;
        public required string PhoneNumber { get; set; }
        public string? SecondPhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
