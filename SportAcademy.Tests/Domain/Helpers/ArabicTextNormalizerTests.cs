using FluentAssertions;
using SportAcademy.Domain.Helpers;
using Xunit;

namespace SportAcademy.Tests.Domain.Helpers
{
    public class ArabicTextNormalizerTests
    {
        [Theory]
        // Hamza-above, hamza-below, madda, wasla all fold to bare alef.
        [InlineData("أحمد", "احمد")]
        [InlineData("إحمد", "احمد")]
        [InlineData("آحمد", "احمد")]
        [InlineData("ٱحمد", "احمد")]
        public void Normalize_FoldsAlefVariantsTogether(string a, string b)
        {
            ArabicTextNormalizer.NormalizedEquals(a, b).Should().BeTrue();
        }

        [Fact]
        public void Normalize_FoldsTaaMarbutaToHaa()
        {
            ArabicTextNormalizer.NormalizedEquals("فاطمة", "فاطمه").Should().BeTrue();
        }

        [Fact]
        public void Normalize_FoldsAlefMaqsuraToYaa()
        {
            ArabicTextNormalizer.NormalizedEquals("مصطفى", "مصطفي").Should().BeTrue();
        }

        [Fact]
        public void Normalize_StripsTatweel()
        {
            // A decorative kashida inserted between letters must not defeat matching.
            ArabicTextNormalizer.Normalize("مـحـمـد").Should().Be(ArabicTextNormalizer.Normalize("محمد"));
        }

        [Fact]
        public void Normalize_StripsHarakat()
        {
            // "Muhammad" fully diacritized (fatha, damma, sukoon) vs. bare.
            ArabicTextNormalizer.Normalize("مُحَمَّدٌ").Should().Be(ArabicTextNormalizer.Normalize("محمد"));
        }

        [Fact]
        public void Normalize_PreservesBaseLettersInTheDiacriticCodeBlock()
        {
            // Regression: base letters (e.g. seen, U+0633) sit inside the same Unicode block as
            // several diacritics and must never be stripped by an overly broad range check.
            ArabicTextNormalizer.Normalize("سالم").Should().Be("سالم");
            ArabicTextNormalizer.Normalize("سلام").Should().Be("سلام");
        }

        [Fact]
        public void Normalize_FoldsArabicIndicDigitsToAscii()
        {
            ArabicTextNormalizer.Normalize("٠١٢٣٤٥٦٧٨٩").Should().Be("0123456789");
        }

        [Fact]
        public void Normalize_FoldsExtendedArabicIndicDigitsToAscii()
        {
            ArabicTextNormalizer.Normalize("۰۱۲۳۴۵۶۷۸۹").Should().Be("0123456789");
        }

        [Fact]
        public void Normalize_CollapsesRepeatedWhitespaceAndTrims()
        {
            ArabicTextNormalizer.Normalize("  احمد   محمد  ").Should().Be("احمد محمد");
        }

        [Fact]
        public void Normalize_UppercasesLatinTextSoMixedInputMatchesCaseInsensitively()
        {
            ArabicTextNormalizer.Normalize("Ahmed").Should().Be("AHMED");
        }

        [Fact]
        public void Normalize_LeavesUnrelatedArabicLettersUntouched()
        {
            // Confidence check: normalization must not over-fold letters that are not variants
            // of each other - "سالم" and "سلام" (different letter order) must stay different.
            ArabicTextNormalizer.NormalizedEquals("سالم", "سلام").Should().BeFalse();
        }

        [Fact]
        public void Normalize_NullOrEmpty_ReturnsEmptyString()
        {
            ArabicTextNormalizer.Normalize(null).Should().Be(string.Empty);
            ArabicTextNormalizer.Normalize("").Should().Be(string.Empty);
        }

        [Fact]
        public void Normalize_RealisticSpellingVariants_MatchAsAUserWouldExpect()
        {
            // "Ahmed" spelled two common ways in casual Arabic typing.
            ArabicTextNormalizer.NormalizedEquals("أحمد المطيري", "احمد المطيرى").Should().BeTrue();
        }
    }
}
