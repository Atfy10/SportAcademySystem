using PhoneNumbers;

namespace SportAcademy.Application.Common.CsvImport
{
    public static class ImportSamplePhones
    {
        // Distinct, valid mobile numbers for the tenant's country (libphonenumber's own example
        // number, varied in its last digit), as plain national digits - a leading "+" is the first
        // thing Excel strips when the file is opened and saved again. Used by the import templates
        // so their sample rows import as-is.
        public static IReadOnlyList<string> For(string countryIso, int count)
        {
            var util = PhoneNumberUtil.GetInstance();
            var example = util.GetExampleNumberForType(countryIso, PhoneNumberType.MOBILE)
                ?? util.GetExampleNumberForType("KW", PhoneNumberType.MOBILE);
            if (example is null) return Enumerable.Repeat(string.Empty, count).ToList();

            string Variant(int delta)
            {
                var national = example.NationalNumber;
                var candidate = new PhoneNumber.Builder()
                    .SetCountryCode(example.CountryCode)
                    .SetNationalNumber(national - (national % 10) + (ulong)((national % 10 + (ulong)delta) % 10))
                    .Build();
                var chosen = util.IsValidNumber(candidate) ? candidate : example;
                return new string(util.Format(chosen, PhoneNumberFormat.NATIONAL).Where(char.IsDigit).ToArray());
            }

            return Enumerable.Range(0, count).Select(Variant).ToList();
        }
    }
}
