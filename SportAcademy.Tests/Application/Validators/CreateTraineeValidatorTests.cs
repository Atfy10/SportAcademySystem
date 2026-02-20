using FluentAssertions;
using FluentValidation.TestHelper;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Validators.TraineeValidators;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Validators;

public class CreateTraineeValidatorTests
{
    private readonly CreateTraineeValidator _validator = new();

    private static CreateTraineeCommand CreateValidCommand() => new()
    {
        FirstName = "Ahmed",
        LastName = "Al-Mutairi",
        SSN = "304031512345",
        BirthDate = new DateOnly(2004, 3, 15),
        Gender = Gender.Male,
        BranchId = 1
    };

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

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

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyLastName_HasError(string? lastName)
    {
        var command = CreateValidCommand() with { LastName = lastName! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

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
    [InlineData("12345678901")] // 11 chars
    [InlineData("1234567890123")] // 13 chars
    public void Validate_SSNWrongLength_HasError(string ssn)
    {
        var command = CreateValidCommand() with { SSN = ssn };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.SSN);
    }

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
}
