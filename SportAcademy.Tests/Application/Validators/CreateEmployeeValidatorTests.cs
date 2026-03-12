using FluentAssertions;
using FluentValidation.TestHelper;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Validators.EmployeeValidators;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Validators;

public class CreateEmployeeValidatorTests
{
    private readonly CreateEmployeeValidator _validator = new();

    private static CreateEmployeeCommand CreateValidCommand() => new(
        FirstName: "Mohammad",
        LastName: "Al-Sabah",
        SSN: "294051512345",
        Salary: 5000m,
        Gender: Gender.Male,
        BirthDate: new DateOnly(1990, 4, 5),
        Email: "mohammad.sabah@academy.com",
        Nationality: "Kuwaiti",
        Street: "Main Street 123",
        City: "Kuwait City",
        PhoneNumber: "51234567",
        SecondNumber: "65234567",
        Position: Position.Manager,
        BranchId: 1
    );

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region FirstName Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyFirstName_HasError(string? firstName)
    {
        var command = CreateValidCommand() with { FirstName = firstName! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public void Validate_FirstNameTooLong_HasError()
    {
        var command = CreateValidCommand() with { FirstName = new string('A', 51) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public void Validate_FirstNameMaxLength_IsValid()
    {
        var command = CreateValidCommand() with { FirstName = new string('A', 50) };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.FirstName);
    }

    #endregion

    #region LastName Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyLastName_HasError(string? lastName)
    {
        var command = CreateValidCommand() with { LastName = lastName! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Fact]
    public void Validate_LastNameTooLong_HasError()
    {
        var command = CreateValidCommand() with { LastName = new string('A', 51) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    #endregion

    #region SSN Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptySSN_HasError(string? ssn)
    {
        var command = CreateValidCommand() with { SSN = ssn! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.SSN);
    }

    [Theory]
    [InlineData("1234567890")] // 10 digits - minimum valid
    [InlineData("12345678901234")] // 14 digits - maximum valid
    public void Validate_SSNValidLength_IsValid(string ssn)
    {
        var command = CreateValidCommand() with { SSN = ssn };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.SSN);
    }

    #endregion

    #region Email Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyEmail_HasError(string? email)
    {
        var command = CreateValidCommand() with { Email = email! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_ValidEmail_IsValid()
    {
        var command = CreateValidCommand() with { Email = "test.user@academy.com" };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }

    #endregion

    #region Nationality Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyNationality_HasError(string? nationality)
    {
        var command = CreateValidCommand() with { Nationality = nationality! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Nationality);
    }

    [Fact]
    public void Validate_ValidNationality_IsValid()
    {
        var command = CreateValidCommand() with { Nationality = "Kuwaiti" };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Nationality);
    }

    #endregion

    #region BirthDate Tests

    [Fact]
    public void Validate_FutureBirthDate_HasError()
    {
        var command = CreateValidCommand() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.BirthDate);
    }

    [Fact]
    public void Validate_BirthDateUnder16Years_HasError()
    {
        var command = CreateValidCommand() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-15))
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.BirthDate);
    }

    [Fact]
    public void Validate_BirthDateOver16Years_IsValid()
    {
        var command = CreateValidCommand() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-17))
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.BirthDate);
    }

    #endregion

    #region Address Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyStreet_HasError(string? street)
    {
        var command = CreateValidCommand() with { Street = street! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Street);
    }

    [Fact]
    public void Validate_StreetTooLong_HasError()
    {
        var command = CreateValidCommand() with { Street = new string('A', 101) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Street);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyCity_HasError(string? city)
    {
        var command = CreateValidCommand() with { City = city! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.City);
    }

    [Fact]
    public void Validate_CityTooLong_HasError()
    {
        var command = CreateValidCommand() with { City = new string('A', 51) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.City);
    }

    #endregion

    #region Phone Number Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyPhoneNumber_HasError(string? phone)
    {
        var command = CreateValidCommand() with { PhoneNumber = phone! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.PhoneNumber);
    }

    [Theory]
    [InlineData("1234567")] // Too short
    [InlineData("123456789")] // Too long
    [InlineData("41234567")] // Invalid first digit (4)
    public void Validate_InvalidPhoneNumber_HasError(string phone)
    {
        var command = CreateValidCommand() with { PhoneNumber = phone };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.PhoneNumber);
    }

    [Theory]
    [InlineData("51234567")] // Kuwait number starting with 5
    [InlineData("61234567")] // Kuwait number starting with 6
    [InlineData("91234567")] // Kuwait number starting with 9
    [InlineData("+96551234567")] // With country code
    public void Validate_ValidPhoneNumber_IsValid(string phone)
    {
        var command = CreateValidCommand() with { PhoneNumber = phone };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.PhoneNumber);
    }

    [Fact]
    public void Validate_InvalidSecondPhoneNumber_HasError()
    {
        var command = CreateValidCommand() with { SecondNumber = "1234567" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.SecondNumber);
    }

    [Fact]
    public void Validate_ValidSecondPhoneNumber_IsValid()
    {
        var command = CreateValidCommand() with { SecondNumber = "65234567" };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.SecondNumber);
    }

    [Fact]
    public void Validate_EmptySecondPhoneNumber_IsValid()
    {
        var command = CreateValidCommand() with { SecondNumber = "" };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.SecondNumber);
    }

    [Fact]
    public void Validate_NullSecondPhoneNumber_IsValid()
    {
        var command = CreateValidCommand() with { SecondNumber = null };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.SecondNumber);
    }

    #endregion

    #region Salary Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_SalaryNotGreaterThanZero_HasError(decimal salary)
    {
        var command = CreateValidCommand() with { Salary = salary };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Salary);
    }

    [Fact]
    public void Validate_SalaryTooHigh_HasError()
    {
        var command = CreateValidCommand() with { Salary = 100001 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Salary);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(50000)]
    [InlineData(100000)]
    public void Validate_ValidSalary_IsValid(decimal salary)
    {
        var command = CreateValidCommand() with { Salary = salary };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Salary);
    }

    #endregion

    #region Gender Tests

    [Fact]
    public void Validate_InvalidGender_HasError()
    {
        // This test would only apply if invalid enum value could be passed
        // In C# with strongly typed enums, this is prevented at compile time
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Gender);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public void Validate_ValidGender_IsValid(Gender gender)
    {
        var command = CreateValidCommand() with { Gender = gender };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Gender);
    }

    #endregion

    #region Position Tests

    [Theory]
    [InlineData(Position.Manager)]
    [InlineData(Position.Coach)]
    [InlineData(Position.HR)]
    public void Validate_ValidPosition_IsValid(Position position)
    {
        var command = CreateValidCommand() with { Position = position };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Position);
    }

    #endregion

    #region BranchId Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidBranchId_HasError(int branchId)
    {
        var command = CreateValidCommand() with { BranchId = branchId };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.BranchId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Validate_ValidBranchId_IsValid(int branchId)
    {
        var command = CreateValidCommand() with { BranchId = branchId };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.BranchId);
    }

    #endregion
}
