using FluentAssertions;
using FluentValidation.TestHelper;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Common.Regional;
using SportAcademy.Application.Validators.EmployeeValidators;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Validators;

public class CreateEmployeeValidatorTests
{
    private readonly CreateEmployeeValidator _validator =
        new(new RegionalValidationService(), new FixedCountryReader("KW"));

    // Employee SSN now goes through the same country-aware National ID rule Trainee already
    // used (CreateTraineeValidator) - Kuwait's real 12-digit checksum, not the old, unrelated
    // "10-14 digits, no checksum" rule this fixture used to satisfy. Century digit '2' (born
    // before 2000) + yyMMdd birth date "900405" (BirthDate below is 1990-04-05) + 5 filler
    // digits.
    private static CreateEmployeeCommand CreateValidCommand() => new(
        FirstName: "Mohammad",
        LastName: "Al-Sabah",
        SSN: "290040512345",
        Salary: 5000m,
        Gender: Gender.Male,
        BirthDate: new DateOnly(1990, 4, 5),
        Email: "mohammad.sabah@academy.com",
        Nationality: "Kuwaiti",
        Street: "Main Street 123",
        City: "Kuwait City",
        PhoneNumber: "51234567",
        SecondNumber: "50012345",
        Position: Position.Manager,
        BranchId: 1
    );

    // ClassLevelCascadeMode.Stop means an SSN checksum failure (SSN is declared before
    // BirthDate) would silently swallow a BirthDate-only assertion below - keep SSN
    // checksum-valid for whatever BirthDate a given test uses.
    private static string ValidKuwaitSsn(DateOnly birthDate)
        => (birthDate.Year > 1999 ? "3" : "2") + birthDate.ToString("yyMMdd") + "12345";

    [Fact]
    public async Task Validate_ValidCommand_HasNoErrors()
    {
        var command = CreateValidCommand();

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region FirstName Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyFirstName_HasError(string? firstName)
    {
        var command = CreateValidCommand() with { FirstName = firstName! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public async Task Validate_FirstNameTooLong_HasError()
    {
        var command = CreateValidCommand() with { FirstName = new string('A', 51) };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public async Task Validate_FirstNameMaxLength_IsValid()
    {
        var command = CreateValidCommand() with { FirstName = new string('A', 50) };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.FirstName);
    }

    #endregion

    #region LastName Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyLastName_HasError(string? lastName)
    {
        var command = CreateValidCommand() with { LastName = lastName! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Fact]
    public async Task Validate_LastNameTooLong_HasError()
    {
        var command = CreateValidCommand() with { LastName = new string('A', 51) };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    #endregion

    #region SSN Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptySSN_HasError(string? ssn)
    {
        var command = CreateValidCommand() with { SSN = ssn! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.SSN);
    }

    [Theory]
    [InlineData("1234567890")] // 10 digits - wrong length for Kuwait's 12-digit National ID
    [InlineData("12345678901234")] // 14 digits - wrong length for Kuwait's 12-digit National ID
    public async Task Validate_SSNWrongLength_HasError(string ssn)
    {
        var command = CreateValidCommand() with { SSN = ssn };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.SSN);
    }

    [Fact]
    public async Task Validate_SSNValidKuwaitFormat_IsValid()
    {
        var command = CreateValidCommand();

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.SSN);
    }

    [Fact]
    public async Task Validate_SSNRightLengthWrongChecksum_HasError()
    {
        // 12 digits, but the birth-date-derived prefix doesn't match BirthDate (1990-04-05).
        var command = CreateValidCommand() with { SSN = "199912312345" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.SSN);
    }

    #endregion

    #region Email Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyEmail_HasError(string? email)
    {
        var command = CreateValidCommand() with { Email = email! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public async Task Validate_ValidEmail_IsValid()
    {
        var command = CreateValidCommand() with { Email = "test.user@academy.com" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }

    #endregion

    #region Nationality Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyNationality_HasError(string? nationality)
    {
        var command = CreateValidCommand() with { Nationality = nationality! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Nationality);
    }

    [Fact]
    public async Task Validate_ValidNationality_IsValid()
    {
        var command = CreateValidCommand() with { Nationality = "Kuwaiti" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Nationality);
    }

    #endregion

    #region BirthDate Tests

    [Fact]
    public async Task Validate_FutureBirthDate_HasError()
    {
        var command = CreateValidCommand() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };
        command = command with { SSN = ValidKuwaitSsn(command.BirthDate) };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.BirthDate);
    }

    [Fact]
    public async Task Validate_BirthDateUnder16Years_HasError()
    {
        var command = CreateValidCommand() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-15))
        };
        command = command with { SSN = ValidKuwaitSsn(command.BirthDate) };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.BirthDate);
    }

    [Fact]
    public async Task Validate_BirthDateOver16Years_IsValid()
    {
        var command = CreateValidCommand() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-17))
        };
        command = command with { SSN = ValidKuwaitSsn(command.BirthDate) };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.BirthDate);
    }

    #endregion

    #region Address Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyStreet_HasError(string? street)
    {
        var command = CreateValidCommand() with { Street = street! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Street);
    }

    [Fact]
    public async Task Validate_StreetTooLong_HasError()
    {
        var command = CreateValidCommand() with { Street = new string('A', 101) };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Street);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyCity_HasError(string? city)
    {
        var command = CreateValidCommand() with { City = city! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.City);
    }

    [Fact]
    public async Task Validate_CityTooLong_HasError()
    {
        var command = CreateValidCommand() with { City = new string('A', 51) };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.City);
    }

    #endregion

    #region Phone Number Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyPhoneNumber_HasError(string? phone)
    {
        var command = CreateValidCommand() with { PhoneNumber = phone! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.PhoneNumber);
    }

    // Phone format is now libphonenumber's real Kuwait numbering-plan check (via
    // IRegionalValidationService), not the old hand-rolled "^[569]\d{7}$" regex - a couple of
    // these fixtures changed because the real numbering plan disagrees with that regex (e.g.
    // "61234567"/"91234567" are not actually allocated Kuwait numbers despite starting with
    // 6/9, and "41234567" IS a valid allocated number despite not starting with 5/6/9).
    [Theory]
    [InlineData("1234567")] // Too short
    [InlineData("123456789")] // Too long
    [InlineData("61234567")] // Not an allocated Kuwait number despite starting with 6
    public async Task Validate_InvalidPhoneNumber_HasError(string phone)
    {
        var command = CreateValidCommand() with { PhoneNumber = phone };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.PhoneNumber);
    }

    [Theory]
    [InlineData("51234567")] // Valid Kuwait mobile number
    [InlineData("50012345")] // libphonenumber's own Kuwait mobile example number
    [InlineData("+96551234567")] // With country code
    public async Task Validate_ValidPhoneNumber_IsValid(string phone)
    {
        var command = CreateValidCommand() with { PhoneNumber = phone };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.PhoneNumber);
    }

    [Fact]
    public async Task Validate_InvalidSecondPhoneNumber_HasError()
    {
        var command = CreateValidCommand() with { SecondNumber = "1234567" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.SecondNumber);
    }

    [Fact]
    public async Task Validate_ValidSecondPhoneNumber_IsValid()
    {
        var command = CreateValidCommand() with { SecondNumber = "50012345" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.SecondNumber);
    }

    [Fact]
    public async Task Validate_EmptySecondPhoneNumber_IsValid()
    {
        var command = CreateValidCommand() with { SecondNumber = "" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.SecondNumber);
    }

    [Fact]
    public async Task Validate_NullSecondPhoneNumber_IsValid()
    {
        var command = CreateValidCommand() with { SecondNumber = null };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.SecondNumber);
    }

    #endregion

    #region Salary Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Validate_SalaryNotGreaterThanZero_HasError(decimal salary)
    {
        var command = CreateValidCommand() with { Salary = salary };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Salary);
    }

    [Fact]
    public async Task Validate_SalaryTooHigh_HasError()
    {
        var command = CreateValidCommand() with { Salary = 100001 };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Salary);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(50000)]
    [InlineData(100000)]
    public async Task Validate_ValidSalary_IsValid(decimal salary)
    {
        var command = CreateValidCommand() with { Salary = salary };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Salary);
    }

    #endregion

    #region Gender Tests

    [Fact]
    public async Task Validate_InvalidGender_HasError()
    {
        // This test would only apply if invalid enum value could be passed
        // In C# with strongly typed enums, this is prevented at compile time
        var command = CreateValidCommand();

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Gender);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task Validate_ValidGender_IsValid(Gender gender)
    {
        var command = CreateValidCommand() with { Gender = gender };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Gender);
    }

    #endregion

    #region Position Tests

    [Theory]
    [InlineData(Position.Manager)]
    [InlineData(Position.Coach)]
    [InlineData(Position.HR)]
    public async Task Validate_ValidPosition_IsValid(Position position)
    {
        var command = CreateValidCommand() with { Position = position };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Position);
    }

    #endregion

    #region BranchId Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidBranchId_HasError(int branchId)
    {
        var command = CreateValidCommand() with { BranchId = branchId };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.BranchId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Validate_ValidBranchId_IsValid(int branchId)
    {
        var command = CreateValidCommand() with { BranchId = branchId };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.BranchId);
    }

    #endregion
}
