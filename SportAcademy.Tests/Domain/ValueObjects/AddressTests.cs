using FluentAssertions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Tests.Domain.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_WithValidInput_ReturnsAddress()
    {
        var address = Address.Create("Street 5, Block 3", "Salmiya");

        address.Street.Should().Be("Street 5, Block 3");
        address.City.Should().Be("Salmiya");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var address = Address.Create("  Street 1  ", "  Hawally  ");

        address.Street.Should().Be("Street 1");
        address.City.Should().Be("Hawally");
    }

    [Theory]
    [InlineData("", "City")]
    [InlineData("Street", "")]
    [InlineData("  ", "City")]
    [InlineData("Street", "   ")]
    [InlineData(null, "City")]
    [InlineData("Street", null)]
    public void Create_WithEmptyOrNullParts_ThrowsInvalidAddressException(string? street, string? city)
    {
        var act = () => Address.Create(street!, city!);

        act.Should().Throw<InvalidAddressException>();
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a1 = Address.Create("Street 1", "Salmiya");
        var a2 = Address.Create("Street 1", "Salmiya");

        a1.Equals(a2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a1 = Address.Create("Street 1", "Salmiya");
        var a2 = Address.Create("Street 2", "Hawally");

        a1.Equals(a2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsFormattedAddress()
    {
        var address = Address.Create("Street 5", "Salmiya");

        address.ToString().Should().Be("Street 5, Salmiya");
    }
}
