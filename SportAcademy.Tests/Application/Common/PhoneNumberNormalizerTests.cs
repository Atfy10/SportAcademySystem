using FluentAssertions;
using Moq;
using SportAcademy.Application.Common.Regional;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Tests.Application.Common;

public class PhoneNumberNormalizerTests
{
    private readonly PhoneNumberNormalizer _sut;
    private readonly Mock<ITenantSettingsCountryReader> _countryReaderMock = new();

    public PhoneNumberNormalizerTests()
    {
        _countryReaderMock.Setup(r => r.GetCountryAsync(It.IsAny<CancellationToken>())).ReturnsAsync("EG");
        _sut = new PhoneNumberNormalizer(new RegionalValidationService(), _countryReaderMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NormalizeAsync_NullOrBlank_ReturnsInputUnchanged(string? input)
    {
        var result = await _sut.NormalizeAsync(input);

        result.Should().Be(input);
    }

    [Fact]
    public async Task NormalizeAsync_ValidNationalFormat_ReturnsE164WithCountryCode()
    {
        var result = await _sut.NormalizeAsync("01001234567");

        result.Should().Be("+201001234567");
    }

    [Fact]
    public async Task NormalizeAsync_AlreadyE164_IsIdempotent()
    {
        var result = await _sut.NormalizeAsync("+201001234567");

        result.Should().Be("+201001234567");
    }

    [Fact]
    public async Task NormalizeAsync_FormattedWithSpaces_StillNormalizesToE164()
    {
        var result = await _sut.NormalizeAsync("010 0123 4567");

        result.Should().Be("+201001234567");
    }

    [Fact]
    public async Task NormalizeAsync_UnparseableInput_FallsBackToOriginalValue()
    {
        var result = await _sut.NormalizeAsync("not-a-phone-number");

        result.Should().Be("not-a-phone-number");
    }
}
