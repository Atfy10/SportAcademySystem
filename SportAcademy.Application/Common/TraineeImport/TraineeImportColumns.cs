using System.Globalization;
using System.Text;

namespace SportAcademy.Application.Common.TraineeImport
{
    // One importable column. Key is the canonical header (what the template writes). A file may
    // use any alias instead - the export's display labels in English or Arabic, the old
    // camelCase/Id headers - so a file exported from the console, or an older template, still
    // imports. "Required" columns must exist in the header row; GuardianName/ParentNumber are
    // conditionally required (under 18) and checked per row.
    public sealed record TraineeImportColumn(string Key, bool Required, params string[] Aliases);

    public static class TraineeImportColumns
    {
        public const string FirstName = "FirstName";
        public const string LastName = "LastName";
        public const string BirthDate = "BirthDate";
        public const string Gender = "Gender";
        public const string Nationality = "Nationality";
        public const string PhoneNumber = "PhoneNumber";
        public const string Email = "Email";
        public const string Branch = "Branch";
        public const string NationalityCategory = "NationalityCategory";
        public const string Sports = "Sports";
        public const string Ssn = "SSN";
        public const string GuardianName = "GuardianName";
        public const string ParentNumber = "ParentNumber";
        public const string Street = "Street";
        public const string City = "City";
        public const string FamilyId = "FamilyId";
        public const string MedicalConditions = "MedicalConditions";

        public static readonly IReadOnlyList<TraineeImportColumn> All =
        [
            new(FirstName, true, "first name", "given name"),
            new(LastName, true, "last name", "family name", "surname"),
            new(BirthDate, true, "birth date", "date of birth", "dob", "birthday"),
            new(Gender, true, "sex"),
            new(Nationality, true),
            new(PhoneNumber, true, "phone", "phone number", "mobile", "mobile number"),
            new(Email, true, "e-mail", "email address"),
            new(Branch, true, "branchid", "branch id", "branch name"),
            new(NationalityCategory, true, "nationalitycategoryid", "nationality category id", "nationality category", "category"),
            new(Sports, false, "sportids", "sport ids", "sport", "sports ids"),
            new(Ssn, false, "national id", "civil id", "ssn number"),
            new(GuardianName, false, "guardian", "guardian name", "parent name"),
            new(ParentNumber, false, "parent number", "parent phone", "guardian phone", "guardian number"),
            new(Street, false, "address", "street address"),
            new(City, false),
            new(FamilyId, false, "family id", "family"),
            new(MedicalConditions, false, "medical conditions", "medical", "conditions", "health conditions"),
        ];

        // Header / free-text comparison key: case-, space-, punctuation- and diacritic-
        // insensitive, Arabic-Indic digits folded to ASCII, tatweel and Arabic letter variants
        // (أ/إ/آ -> ا, ة -> ه, ى -> ي) unified, so "Nationality Category", "nationality_category"
        // and "فئة  الجنسية" all compare equal to their canonical forms.
        public static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var s = DigitNormalizer.ToAscii(value.Trim().Trim('﻿')).Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat == UnicodeCategory.NonSpacingMark) continue;   // accents, harakat
                if (ch is ' ' or '_' or '-' or '.' or '\t' or 'ـ') continue;   // incl. tatweel
                sb.Append(ch switch
                {
                    'أ' or 'إ' or 'آ' => 'ا',
                    'ة' => 'ه',
                    'ى' => 'ي',
                    _ => char.ToLowerInvariant(ch),
                });
            }
            return sb.ToString();
        }
    }

    public static class DigitNormalizer
    {
        // Arabic-Indic (٠-٩) and Eastern Arabic-Indic (۰-۹) digits to ASCII, and the Arabic
        // decimal/thousands separators to '.'/'' - Excel with an Arabic keyboard produces these.
        public static string ToAscii(string value)
        {
            var sb = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (ch >= '٠' && ch <= '٩') sb.Append((char)('0' + (ch - '٠')));
                else if (ch >= '۰' && ch <= '۹') sb.Append((char)('0' + (ch - '۰')));
                else if (ch == '٫') sb.Append('.');
                else if (ch == '٬') continue;
                else sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
