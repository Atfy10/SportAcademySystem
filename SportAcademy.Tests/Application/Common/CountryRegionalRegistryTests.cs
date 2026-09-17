using FluentAssertions;
using SportAcademy.Application.Common.Regional;

namespace SportAcademy.Tests.Application.Common;

public class CountryRegionalRegistryTests
{
    // Kuwait's checksum is covered separately via PersonValidationHelperTests / the Trainee and
    // Employee validator tests (it delegates to the pre-existing, unchanged helper). These cover
    // the generalized "first 7 digits = century + YYMMDD birth date" cross-check shared by every
    // other launch country.

    public static IEnumerable<object[]> NonKuwaitCountries =>
        new[] { "EG", "SA", "AE", "BH", "QA", "OM" }.Select(c => new object[] { c });

    [Theory]
    [MemberData(nameof(NonKuwaitCountries))]
    public void IsValidNationalId_PrefixMatchesActualBirthDate_IsValid(string iso)
    {
        var birthDate = new DateOnly(1990, 4, 5); // century '2', "900405"
        var rule = CountryRegionalRegistry.GetNationalIdRule(iso);
        var id = "2900405" + new string('1', rule.FixedLength!.Value - 7);

        rule.IsValid(id, birthDate).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(NonKuwaitCountries))]
    public void IsValidNationalId_PrefixDoesNotMatchActualBirthDate_HasError(string iso)
    {
        var birthDate = new DateOnly(1990, 4, 5); // encodes as "2900405"
        var rule = CountryRegionalRegistry.GetNationalIdRule(iso);
        // Right length/digits-only, but the embedded date (900101) doesn't match birthDate.
        var id = "2900101" + new string('1', rule.FixedLength!.Value - 7);

        rule.IsValid(id, birthDate).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(NonKuwaitCountries))]
    public void IsValidNationalId_NoBirthDateProvided_HasError(string iso)
    {
        var rule = CountryRegionalRegistry.GetNationalIdRule(iso);
        var id = "2900405" + new string('1', rule.FixedLength!.Value - 7);

        rule.IsValid(id, null).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(NonKuwaitCountries))]
    public void IsValidNationalId_Born2000OrLater_UsesCenturyDigit3(string iso)
    {
        var birthDate = new DateOnly(2005, 12, 31); // century '3', "051231"
        var rule = CountryRegionalRegistry.GetNationalIdRule(iso);
        var id = "3051231" + new string('1', rule.FixedLength!.Value - 7);

        rule.IsValid(id, birthDate).Should().BeTrue();
    }

    [Fact]
    public void GetNationalIdRule_UnknownCountry_FallsBackToGenericDigitsOnlyRule()
    {
        var rule = CountryRegionalRegistry.GetNationalIdRule("ZZ");

        rule.IsValid("123456", null).Should().BeTrue();
        rule.IsValid("abc123", null).Should().BeFalse();
        rule.IsValid(null, null).Should().BeTrue(); // optional/never required
    }
}
