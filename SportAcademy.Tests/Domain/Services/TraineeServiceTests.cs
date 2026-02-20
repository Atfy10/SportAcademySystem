using FluentAssertions;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Tests.Domain.Services;

public class TraineeServiceTests
{
    private readonly TraineeService _sut = new();

    [Fact]
    public void CalculateAge_ReturnsCorrectAge()
    {
        var birthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-20));

        _sut.CalculateAge(birthDate).Should().Be(20);
    }

    [Fact]
    public void CalculateAge_BirthdayLaterThisYear_ReturnsAgeMinus1()
    {
        // Birthday hasn't happened yet this year
        var futureThisYear = DateTime.Now.AddDays(30);
        var birthDate = new DateOnly(futureThisYear.Year - 25, futureThisYear.Month, futureThisYear.Day);

        _sut.CalculateAge(birthDate).Should().Be(24);
    }

    [Theory]
    [InlineData(-20, true)]
    [InlineData(-15, true)]
    [InlineData(-14, false)]
    [InlineData(-10, false)]
    public void IsAdult_ReturnsTrueForAgeGreaterOrEqual15(int yearsAgo, bool expected)
    {
        var birthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(yearsAgo));

        _sut.IsAdult(birthDate).Should().Be(expected);
    }

    [Fact]
    public void CreateTraineeCode_GeneratesCorrectFormat()
    {
        var trainee = new Trainee
        {
            FirstName = "Ahmed",
            LastName = "Al-Mutairi",
            SSN = "304031512345",
            PhoneNumber = "51234567",
            BirthDate = new DateOnly(2004, 3, 15),
            Gender = Gender.Male
        };

        var code = _sut.CreateTraineeCode(trainee, branchId: 2);

        // branchId=2, dob=0403, ascii of 'A'=65
        // prefix = "2" + "0403" + "65" = "2040365"
        // counter = "01"
        // result = 204036501
        code.Should().Be(204036501);
    }

    [Fact]
    public void IsSSNValid_ValidSSN_ReturnsTrue()
    {
        var birthDate = new DateOnly(2004, 3, 15);
        var ssn = "304031512345";

        _sut.IsSSNValid(ssn, birthDate).Should().BeTrue();
    }

    [Fact]
    public void IsSSNValid_InvalidSSN_ReturnsFalse()
    {
        var birthDate = new DateOnly(2004, 3, 15);
        var ssn = "invalid";

        _sut.IsSSNValid(ssn, birthDate).Should().BeFalse();
    }
}
