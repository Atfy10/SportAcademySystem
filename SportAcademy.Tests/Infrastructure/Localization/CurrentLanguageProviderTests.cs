using FluentAssertions;
using SportAcademy.Infrastructure.Services;
using Xunit;

namespace SportAcademy.Tests.Infrastructure.Localization
{
    public class CurrentLanguageProviderTests
    {
        [Theory]
        // TenantSettings seeds "ar-KW" while the catalogs are neutral "ar".
        [InlineData("ar-KW", "ar")]
        [InlineData("ar", "ar")]
        [InlineData("AR", "ar")]
        [InlineData("en-US", "en")]
        [InlineData("en", "en")]
        public void Normalize_ReducesRegionalTagsToSupportedNeutralCode(string input, string expected)
        {
            CurrentLanguageProvider.Normalize(input).Should().Be(expected);
        }

        [Theory]
        [InlineData("fr-FR")]
        [InlineData("de")]
        [InlineData("not-a-language")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Normalize_FallsBackToEnglishForAnythingUnsupported(string? input)
        {
            CurrentLanguageProvider.Normalize(input).Should().Be(CurrentLanguageProvider.Default);
        }

        [Fact]
        public void Language_DefaultsToEnglishBeforeAnythingIsSet()
        {
            new CurrentLanguageProvider().Language.Should().Be("en");
        }

        [Fact]
        public void SetLanguage_NormalizesBeforeStoring()
        {
            var provider = new CurrentLanguageProvider();

            provider.SetLanguage("ar-KW");

            provider.Language.Should().Be("ar");
        }

        [Fact]
        public void SetLanguage_WithUnsupportedValue_FallsBackRatherThanStoringIt()
        {
            var provider = new CurrentLanguageProvider();

            provider.SetLanguage("fr");

            provider.Language.Should().Be("en");
        }
    }
}
