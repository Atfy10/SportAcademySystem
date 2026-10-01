using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System.Globalization;

namespace SportAcademy.Application.Common.CsvImport
{
    // Cell parsing shared by the CSV import validators: dates as spreadsheets write them, references
    // by id or by name in either language, multi-value cells, and enums by name/number/label.
    public static class ImportValueParser
    {
        private static readonly string[] DateFormats =
        [
            "yyyy-MM-dd", "yyyy/MM/dd", "yyyy-M-d", "yyyy/M/d",
            "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "dd.MM.yyyy", "d.M.yyyy",
            "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss",
        ];

        // ISO and day-first text dates, plus Excel's serial day numbers (a date cell saved as a number).
        public static bool TryParseDate(string raw, out DateOnly date)
        {
            var text = DigitNormalizer.ToAscii(raw);
            if (DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                || (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial)
                    && serial is > 1 and < 80000 && (dt = DateTime.FromOADate(serial)) != default))
            {
                date = DateOnly.FromDateTime(dt);
                return true;
            }
            date = default;
            return false;
        }

        public static IEnumerable<string> SplitList(string value)
            => value.Split(['|', ';', '،', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // By id (a number) or by name in any language, ignoring case, spacing and letter variants.
        public static ImportNamedItem? Resolve(IReadOnlyList<ImportNamedItem> items, string value)
        {
            var ascii = DigitNormalizer.ToAscii(value).Trim();
            if (int.TryParse(ascii, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                var byId = items.FirstOrDefault(i => i.Id == id);
                if (byId is not null) return byId;
            }

            var key = ImportText.Normalize(value);
            return items.FirstOrDefault(i => ImportText.Normalize(i.Name) == key)
                ?? items.FirstOrDefault(i => i.AlternateNames.Any(n => ImportText.Normalize(n) == key));
        }

        // Enum name, number, and its English/Arabic labels.
        public static Dictionary<string, TEnum> EnumMatcher<TEnum>(ILocalizationService localizer) where TEnum : struct, Enum
        {
            var map = new Dictionary<string, TEnum>();
            foreach (var value in Enum.GetValues<TEnum>())
            {
                var labelKey = $"enum.{typeof(TEnum).Name}.{value}";
                foreach (var name in new[]
                {
                    value.ToString(),
                    Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
                    localizer.GetIn("en", labelKey),
                    localizer.GetIn("ar", labelKey),
                })
                {
                    var key = ImportText.Normalize(name);
                    if (key.Length > 0 && !key.StartsWith("enum")) map.TryAdd(key, value);
                }
            }
            if (typeof(TEnum) == typeof(Gender))
            {
                // Common single-letter / alternate spellings.
                map.TryAdd("m", (TEnum)(object)Gender.Male);
                map.TryAdd("f", (TEnum)(object)Gender.Female);
                map.TryAdd("ذ", (TEnum)(object)Gender.Male);
                map.TryAdd("ا", (TEnum)(object)Gender.Female);
            }
            return map;
        }

        public static int AgeOn(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age;
        }
    }
}
