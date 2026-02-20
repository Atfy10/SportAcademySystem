using FluentAssertions;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("TEST@DOMAIN.ORG")]
    [InlineData("name.surname@company.co")]
    public void Create_WithValidEmail_ReturnsEmailWithNormalizedValue(string input)
    {
        var email = Email.Create(input);

        email.Value.Should().Be(input.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespace_ThrowsException(string? input)
    {
        var act = () => Email.Create(input!);

        act.Should().Throw<Exception>();
    }

    [Theory]
    [InlineData("no-at-sign")]
    [InlineData("missing@domain")]
    [InlineData("@missing-local.com")]
    public void Create_WithInvalidFormat_ThrowsException(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Equals_SameEmailDifferentCase_ReturnsTrue()
    {
        var email1 = Email.Create("User@Example.COM");
        var email2 = Email.Create("user@example.com");

        email1.Equals(email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentEmails_ReturnsFalse()
    {
        var email1 = Email.Create("a@example.com");
        var email2 = Email.Create("b@example.com");

        email1.Equals(email2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameEmailDifferentCase_ReturnsSameHash()
    {
        var email1 = Email.Create("User@Example.COM");
        var email2 = Email.Create("user@example.com");

        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        var email = Email.Create("User@Example.COM");

        email.ToString().Should().Be("user@example.com");
    }
}
