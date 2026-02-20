using FluentAssertions;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Tests.Domain.Helpers;

public class PersonValidationHelperTests
{
    [Fact]
    public void IsValidSSN_ValidSSNForPost2000Birth_ReturnsTrue()
    {
        // Born 2004-03-15 → prefix "3040315" then 5 more digits = 12 chars
        var birthDate = new DateOnly(2004, 3, 15);
        var ssn = "304031512345";

        PersonValidationHelper.IsValidSSN(ssn, birthDate).Should().BeTrue();
    }

    [Fact]
    public void IsValidSSN_ValidSSNForPre2000Birth_ReturnsTrue()
    {
        // Born 1985-11-20 → prefix "2851120" then 5 more digits = 12 chars
        var birthDate = new DateOnly(1985, 11, 20);
        var ssn = "285112012345";

        PersonValidationHelper.IsValidSSN(ssn, birthDate).Should().BeTrue();
    }

    [Fact]
    public void IsValidSSN_WrongLength_ReturnsFalse()
    {
        var birthDate = new DateOnly(2004, 3, 15);
        var ssn = "30403151234"; // 11 chars instead of 12

        PersonValidationHelper.IsValidSSN(ssn, birthDate).Should().BeFalse();
    }

    [Fact]
    public void IsValidSSN_ContainsNonDigits_ReturnsFalse()
    {
        var birthDate = new DateOnly(2004, 3, 15);
        var ssn = "30403151234A";

        PersonValidationHelper.IsValidSSN(ssn, birthDate).Should().BeFalse();
    }

    [Fact]
    public void IsValidSSN_WrongPrefix_ReturnsFalse()
    {
        // Born 2004 (post-2000) should start with 3, not 2
        var birthDate = new DateOnly(2004, 3, 15);
        var ssn = "204031512345";

        PersonValidationHelper.IsValidSSN(ssn, birthDate).Should().BeFalse();
    }

    [Fact]
    public void IsValidSSN_WrongBirthDateDigits_ReturnsFalse()
    {
        var birthDate = new DateOnly(2004, 3, 15);
        // Wrong month (05 instead of 03)
        var ssn = "304051512345";

        PersonValidationHelper.IsValidSSN(ssn, birthDate).Should().BeFalse();
    }
}
