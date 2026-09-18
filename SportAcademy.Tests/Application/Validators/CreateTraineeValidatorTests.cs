using FluentAssertions;
using FluentValidation.TestHelper;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Common.Regional;
using SportAcademy.Application.Validators.TraineeValidators;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Validators;

public class CreateTraineeValidatorTests
{
    private readonly CreateTraineeValidator _validator =
        new(new RegionalValidationService(), new FixedCountryReader("KW"));

    private static CreateTraineeCommand CreateValidCommand() => new()
    {
        FirstName = "Ahmed",
        LastName = "Al-Mutairi",
        SSN = "304031512345",
        BirthDate = new DateOnly(2004, 3, 15),
        Gender = Gender.Male,
        BranchId = 1,
        NationalityCategoryId = 1,
        FamilyId = 0,
        PhoneNumber = "51234567",
        Email = "ahmed@example.com",
        Nationality = Nationality.Kuwaiti,
        SportIds = [1]
    };

    [Fact]
    public async Task Validate_ValidCommand_HasNoErrors()
    {
        var command = CreateValidCommand();

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

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

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyLastName_HasError(string? lastName)
    {
        var command = CreateValidCommand() with { LastName = lastName! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    // SSN is deliberately optional on trainee creation (see CreateTraineeValidator's own
    // comment on the SSN rule) - not every trainee has an SSN/civil ID on file yet.
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptySSN_IsValid(string? ssn)
    {
        var command = CreateValidCommand() with { SSN = ssn! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.SSN);
    }

    [Theory]
    [InlineData("12345678901")] // 11 chars
    [InlineData("1234567890123")] // 13 chars
    public async Task Validate_SSNWrongLength_HasError(string ssn)
    {
        var command = CreateValidCommand() with { SSN = ssn };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.SSN);
    }

    [Fact]
    public async Task Validate_FutureBirthDate_HasError()
    {
        var command = CreateValidCommand() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.BirthDate);
    }
}
