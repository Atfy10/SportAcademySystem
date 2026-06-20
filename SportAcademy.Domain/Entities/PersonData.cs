using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Domain.Entities
{
    public record PersonData(
        string FirstName,
        string LastName,
        string SSN,
        Email Email,
        DateOnly BirthDate,
        Gender Gender,
        Nationality Nationality,
        Address Address,
        string PhoneNumber,
        string? SecondPhoneNumber
    );
}
