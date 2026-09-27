using FluentAssertions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Tests.Domain.Services;

public class PersonServiceTests
{
    private readonly PersonService _sut = new();

    [Fact]
    public void CalculateAge_ReturnsCorrectAge()
    {
        var birthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-30));

        _sut.CalculateAge(birthDate).Should().Be(30);
    }

    [Fact]
    public void CalculateAge_WithExplicitAsOf_UsesItInsteadOfServerLocalTime()
    {
        // Birthday is tomorrow relative to asOf - CalculateAge must resolve "today" from asOf,
        // not from DateTime.Now, or a caller passing the tenant's own local date (e.g. via
        // ITenantClock) could never actually override the server's timezone.
        var asOf = new DateOnly(2026, 6, 14);
        var birthDate = new DateOnly(1996, 6, 15);

        _sut.CalculateAge(birthDate, asOf).Should().Be(29);
    }

    [Fact]
    public void GenerateUserName_ReturnsExpectedFormat()
    {
        var userName = _sut.GenerateUserName("Ahmed", "Al-Mutairi");

        // Should be lowercase: firstnamelasttwo_XX
        userName.Should().StartWith("ahmedal");
        userName.Should().Contain("_");
        userName.Length.Should().BeGreaterThan(5);
    }

    [Fact]
    public void GeneratePassword_Returns12CharString()
    {
        var password = _sut.GeneratePassword();

        password.Length.Should().Be(12);
    }

    [Fact]
    public void GeneratePassword_ContainsVariousCharacters()
    {
        // Generate several and check they are not all the same
        var passwords = Enumerable.Range(0, 10).Select(_ => _sut.GeneratePassword()).ToList();

        passwords.Distinct().Count().Should().BeGreaterThan(1);
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
        var ssn = "000000000000";

        _sut.IsSSNValid(ssn, birthDate).Should().BeFalse();
    }
}
